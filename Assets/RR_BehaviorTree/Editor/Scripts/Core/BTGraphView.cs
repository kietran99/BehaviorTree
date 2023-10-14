using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTGraphView : AbstractGraphView
    {
        private readonly Vector2 DEFAULT_ROOT_SPAWN_POS = new Vector2(400f, 480f);
        private readonly Rect NODE_INFO_RECT = new Rect(10, 30, 320, 560);
        private readonly Rect BB_RECT = new Rect(10, 30 + 560 + 10, 320, 400);
        private readonly Rect SETTINGS_RECT = new Rect(10f, 30f, 400f, 400f);

        private SerializedObject _serializedGraphDesign;
        private readonly GraphBlackboard _blackboard;
        private readonly BTSubWndGraphDetails _nodeDetails;
        private BTSubWndGraphSettings _graphSettingsWnd;
        private readonly BTDecoratorSearchWindow _decoSearchWnd;
        private readonly BTServiceSearchWindow _serviceSearchWnd;
        private Debugger.BTVisualDebugger _debugger;
        private AI.Debugger.BBVisualDebugger _BBDebugger;
        private List<BTGraphNodeBase> _graphNodes;

        // public static Action<string> OnNodeSelected { get; set; }
        public static Action<ElementSelectParams> OnNewElementSelected { get; set; }
        // public Action OnNodeDeleted { get; set; }

        public BTGraphDesign GraphDesign => _serializedGraphDesign.targetObject as BTGraphDesign;

        public BTGraphView(BTGraphDesign designContainer) : base()
        {
            _blackboard = CreateBlackboard(this, designContainer.Blackboard, designContainer.Blackboard, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new BTSubWndGraphDetails(NODE_INFO_RECT);
            Add(_nodeDetails);
            Init(designContainer);
            _decoSearchWnd = CreateAttacherSearchWindow<BTDecoratorSearchWindow>();
            _serviceSearchWnd = CreateAttacherSearchWindow<BTServiceSearchWindow>();

            graphViewChanged += OnGraphViewChanged;
            OnNewElementSelected += HandleNewElementSelected;

            BTEditorWindow.OnClose += () =>
            {
                if (OnNewElementSelected == null)
                {
                    return;
                }

                foreach (var listener in OnNewElementSelected.GetInvocationList())
                {
                    OnNewElementSelected -= (Action<ElementSelectParams>)listener;
                }
            };  
        }

        public void Init(BTGraphDesign designContainer)
        {
            _serializedGraphDesign = new SerializedObject(designContainer);

            if (designContainer.NodeDataList == null || designContainer.NodeDataList.Count == 0)
            {
                var initParams = new BTGraphInitParamsNode() { pos = DEFAULT_ROOT_SPAWN_POS, blackboard = _blackboard };
                var root = BTGraphNodeFactory.CreateGraphNode(BTNodeType.Root, initParams);
                AddNode(root, DEFAULT_ROOT_SPAWN_POS);
                return;
            }

            var linkDataList = new List<BTLinkData>(designContainer.NodeDataList.Count);
            var nodeDict = new Dictionary<string, Node>(designContainer.NodeDataList.Count);

            System.Func<BTNodeType, Type> GetGraphNodeType = nodeType =>
            {
                switch (nodeType)
                {
                    case BTNodeType.Root:
                        return typeof(BTGraphRoot);
                    case BTNodeType.Selector:
                        return typeof(BTGraphSelector);
                    case BTNodeType.Sequencer:
                        return typeof(BTGraphSequencer);
                    case BTNodeType.Leaf:
                    default:
                        return typeof(BTGraphRoot);
                }
            };

            Func<BTSerializableNodeData, BTGraphNodeBase> CreateGraphNode = nodeData =>
            {
                var initParams = new BTGraphInitParamsNode()
                {
                    pos = nodeData.Position, 
                    blackboard = _blackboard, 
                    name = nodeData.Name, 
                    desc = nodeData.Description, 
                    guid = nodeData.Guid,
                    icon = BTGlobalSettings.Instance.GetIcon(GetGraphNodeType(nodeData.NodeType)),
                    OpenDecoSearchWindow = OpenDecoSearchWnd,
                    OpenServiceSearchWindow = OpenServiceSearchWnd
                };

                var node = BTGraphNodeFactory.CreateGraphNode(nodeData.NodeType, initParams);
                return node;
            };

            Func<BTSerializableTaskData, BTGraphNodeBase> CreateGraphNodeLeaf = taskData =>
            {
                var initParams = new BTGraphInitParamsNodeLeaf()
                {
                    task = taskData.Task,
                    pos = taskData.Position,
                    blackboard = _blackboard,
                    name = taskData.Name,
                    desc = taskData.Description,
                    guid = taskData.Guid,
                    icon = BTGlobalSettings.Instance.GetIcon(taskData.Task.GetType()),
                    OpenDecoSearchWindow = OpenDecoSearchWnd,
                    OpenServiceSearchWindow = OpenServiceSearchWnd
                };

                var node = BTGraphNodeFactory.CreateGraphNodeLeaf(initParams);
                return node;
            };

            _graphNodes = new BTExecListBuilder<BTSerializableNodeDataBase, BTGraphNodeBase>()
                .OnObjectCreate((node, parentGuid) =>
                {
                    if (GraphDesign.TryGetAttachers(node.Guid, out List<BTSerializableAttacher> attachers))
                    {
                        node.InitAttachers(attachers);
                    }

                    nodeDict.Add(node.Guid, node);
                
                    if (!string.IsNullOrEmpty(parentGuid)) 
                    {
                        linkDataList.Add(new BTLinkData() { startGuid = parentGuid, endGuid = node.Guid });
                    }

                    AddElement(node);
                })
                .OnObjectOrder((node, idx, parentIdx) =>
                {
                    var orderLb = new BTGraphOrderLabel(node, idx);
                    AddElement(orderLb);
                    node.OrderValue = idx;
                    var attacher = new Attacher(orderLb, node, SpriteAlignment.TopRight)
                    {
                        distance = -13.0f
                    };
                    attacher.Reattach();
                })
                .Execute(
                    designContainer.AsNodeDataBaseList,
                    data => 
                    {
                        bool isTaskNode = data.GetType().Equals(typeof(BTSerializableTaskData));
                        return isTaskNode 
                                ? CreateGraphNodeLeaf(data as BTSerializableTaskData)
                                : CreateGraphNode(data as BTSerializableNodeData);
                    }
                );

            linkDataList.ForEach(linkData =>
            {
                var (parent, child) = (nodeDict[linkData.startGuid], nodeDict[linkData.endGuid]);
                var edge = (child.inputContainer[0] as Port).ConnectTo(parent.outputContainer[0] as Port);
                AddElement(edge);
            });

            BTGraphNodeBase.AttacherDeleted += OnAttacherDelete;
        }

        private void OnAttacherDelete(string nodeGuid, string attacherToDeleteGuid, Action onSuccessCallback)
        {
            bool deleteSuccess = GraphDesign.DeleteAttacher(nodeGuid, attacherToDeleteGuid);

            if (!deleteSuccess)
            {
                return;
            }

            onSuccessCallback();
        }

        private T CreateAttacherSearchWindow<T>() where T : BTSearchWindowBase
        {
            var wnd = ScriptableObject.CreateInstance<T>();
            wnd.Init();
            return wnd;
        }

        private void OpenDecoSearchWnd(string decorateeGuid, Vector2 pos, Action<BTGraphInitParamsAttacher> onEntrySelectCb)
        {
            OpenAttacherSearchWnd(_decoSearchWnd, decorateeGuid, pos, onEntrySelectCb);
        }

        private void OpenServiceSearchWnd(string decorateeGuid, Vector2 pos, Action<BTGraphInitParamsAttacher> onEntrySelectCb)
        {
            OpenAttacherSearchWnd(_serviceSearchWnd, decorateeGuid, pos, onEntrySelectCb);
        }

        private void OpenAttacherSearchWnd<T>(
            T window, 
            string targetGuid, 
            Vector2 pos, 
            Action<BTGraphInitParamsAttacher> onEntrySelectCb)
            where T : BTSearchWindowBase
        {
            window.OnEntrySelected = (type, pos) =>
                {
                    BTTaskBase attacherSO = BTGlobalSettings.Instance.PlaygroundMode
                        ? GraphDesign.CreateDummyTask(type)
                        : GraphDesign.TaskCtor(type);

                    string attacherGuid = Guid.NewGuid().ToString();
                    var initParams = new BTGraphInitParamsAttacher()
                    {
                        guid = attacherGuid,
                        nodeID = targetGuid,
                        name = attacherSO.Name,
                        icon = BTGlobalSettings.Instance.GetIcon(type),
                        task = attacherSO
                    };

                    onEntrySelectCb(initParams);

                    if (BTGlobalSettings.Instance.PlaygroundMode)
                    {
                        return;
                    }
                    
                    GraphDesign.AddAttacher(targetGuid, new BTSerializableAttacher(attacherGuid, attacherSO.Name, attacherSO));
                };

            SearchWindow.Open(new SearchWindowContext(pos), window);
        }

        public void AttachVisualDebugger(BTScheduler scheduler)
        {
            _debugger = new Debugger.BTVisualDebugger(scheduler, _graphNodes);
            _BBDebugger = new AI.Debugger.BBVisualDebugger(_blackboard);
        }

        private void HandleNewElementSelected(ElementSelectParams nodeSelectParams)
        {
            if (nodeSelectParams.IsAttacher)
            {
                OnSelectedAttacher(nodeSelectParams);
                return;
            }

            OnSelectedNode(nodeSelectParams);
        }

        private void OnSelectedNode(ElementSelectParams elementSelectParams)
        {
            bool isMultiSelect = selection.Count > 1;
            if (isMultiSelect || typeof(BTTaskBase).IsAssignableFrom(selection[0].GetType()))
            {
                return;
            }

            Func<string, string, SerializedObject, SerializedProperty> FindPropName = (guidToFind, dataListPropName, graphDesign) =>
            {
                SerializedProperty nodeDataList = graphDesign.FindProperty(dataListPropName);
                foreach (SerializedProperty nodeData in nodeDataList)
                {
                    SerializedProperty nodeGraphData = nodeData.FindPropertyRelative("_graphData");
                    SerializedProperty guid = nodeGraphData.FindPropertyRelative("_guid");
                    if (guidToFind == guid.stringValue)
                    {
                        return nodeGraphData.FindPropertyRelative("_name");
                    }
                }

                Debug.LogWarning($"Invalid Guid: {guidToFind}");
                return null;
            };

            string dataListPropName = elementSelectParams.Task == null ? "_nodeDataList" : "_taskDataList";
            SerializedProperty propName = FindPropName(elementSelectParams.Guid, dataListPropName, _serializedGraphDesign);

            if (propName == null)
            {
                return;
            }

            BTGraphNodeBase selectedElement = selection[0] as BTGraphNodeBase;
            _nodeDetails.ShowNodeInfo(propName, newName => selectedElement.Rename(newName));
            
            if (elementSelectParams.Task != null)
            {
                _nodeDetails.DrawTaskProp(elementSelectParams.Task, _blackboard);
            }
            else
            {
                _nodeDetails.ClearTaskPropsContent();
            }
        }

        private void OnSelectedAttacher(ElementSelectParams elementSelectParams)
        {
            Func<string, SerializedObject, (SerializedProperty, string)> FindPropNameAndDecorateeGuid = (guidToFind, graphDesign) =>
            {
                SerializedProperty attacherDictInternalList = graphDesign.FindProperty("_attacherDict").FindPropertyRelative("_entries");
                foreach (SerializedProperty keyValuePair in attacherDictInternalList)
                {
                    SerializedProperty attacherList = keyValuePair.FindPropertyRelative("Value");
                    foreach (SerializedProperty attacher in attacherList)
                    {
                        string guid = attacher.FindPropertyRelative("guid").stringValue;
                        if (guidToFind == guid)
                        {
                            SerializedProperty propName = attacher.FindPropertyRelative("name");
                            string decorateeGuid = keyValuePair.FindPropertyRelative("Key").stringValue;
                            return (propName, decorateeGuid);
                        }
                    }
                }

                Debug.LogError($"Invalid Guid: {guidToFind}");
                return (null, string.Empty);
            };

            (SerializedProperty propName, string decorateeGuid) = FindPropNameAndDecorateeGuid(elementSelectParams.Guid, _serializedGraphDesign);
            BTGraphNodeBase decorateeNode = _graphNodes.Find(node => node.Guid == decorateeGuid);
            BTGraphNodeAttacher selectedAttacher = decorateeNode.FindAttacher(elementSelectParams.Guid);
            _nodeDetails.ShowNodeInfo(propName, newName => selectedAttacher.Rename(newName));
            _nodeDetails.DrawTaskProp(elementSelectParams.Task, _blackboard);
        }

        private GraphBlackboard CreateBlackboard(
            BTGraphView graphView, 
            Blackboard blackboard, 
            ScriptableObject BBContainer,
            string title, 
            Rect rect)
        {
            var graphBlackboard = new GraphBlackboard(blackboard, BBContainer, graphView) { title = title, scrollable = true };
            graphBlackboard.SetPosition(rect);      
            return graphBlackboard;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            Action<List<GraphElement>> OnElementsRemoved = elementsToRemove =>
            {
                if (elementsToRemove == null)
                {
                    return;
                }
                
                foreach (var element in elementsToRemove)
                {
                    if (element is IBTSerializableNode)
                    {
                        (element as IBTSerializableNode).OnDelete(GraphDesign);
                    }
                }
            };

            Action<List<GraphElement>> OnElementsMoved = movedElements =>
            {
                if (movedElements == null)
                {
                    return;
                }

                for (int i = 0; i < movedElements.Count; i++)
                {
                    GraphElement element = movedElements[i];

                    if (element is Node)
                    {
                        var node = (element as IBTSerializableNode);
                        node.OnMove(GraphDesign, graphViewChange.moveDelta);
                    }
                }
            };

            Action<List<Edge>> OnEdgesCreated = edgesToCreate =>
            {
                if (edgesToCreate == null)
                {
                    return;
                }

                for (int i = 0; i < edgesToCreate.Count; i++)
                {
                    var inputNode = edgesToCreate[i].output.node as IBTSerializableNode;
                    var outputNode = edgesToCreate[i].input.node as IBTSerializableNode;
                    outputNode.OnConnect(GraphDesign, inputNode.Guid);
                }
            };

            OnElementsRemoved(graphViewChange.elementsToRemove);
            OnElementsMoved(graphViewChange.movedElements);
            OnEdgesCreated(graphViewChange.edgesToCreate);

            return graphViewChange;
        }

        public void AddNode(Node node, Vector2 pos)
        {
            AddElement(node);
            var convertedNode = node as BTGraphNodeBase;
            // var orderLb = new BTGraphOrderLabel(node as BTGraphNodeBase, 0);
            // AddElement(orderLb);
            // orderLb.visible = false;
            // var attacher = new Attacher(orderLb, node, SpriteAlignment.TopRight);
            // attacher.distance = -13.0f;
            // attacher.Reattach();
            convertedNode.OnCreate(GraphDesign, pos);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new System.Collections.Generic.List<Port>();

            ports.ForEach(port => 
            {
                if (startPort.node != port.node && startPort != port)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public override UnityEditor.Experimental.GraphView.Blackboard GetBlackboard() => _blackboard;

        public void OpenGraphSettingsWnd()
        {
            if (_graphSettingsWnd == null)
            {
                (float width, float height) = (SETTINGS_RECT.width, SETTINGS_RECT.height);
                (float x, float y) = (contentRect.width - width - SETTINGS_RECT.x, SETTINGS_RECT.y);

                _graphSettingsWnd = new BTSubWndGraphSettings(
                    BTGlobalSettings.Instance.NodeIconSettingsAsset,
                    () => RemoveElement(_graphSettingsWnd),
                    new Rect(x, y, width, height));
            }

            _graphSettingsWnd.Open();
            Add(_graphSettingsWnd);
        }

        public void Cleanup()
        {
            BTGraphNodeBase.AttacherDeleted -= OnAttacherDelete;
        }

        public void SetDetailsSubWndVisible(bool value) => _nodeDetails.visible = value;
    }
}
