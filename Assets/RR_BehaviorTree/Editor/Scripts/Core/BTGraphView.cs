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

        private SerializedObject _serializedDesContainer;
        private GraphBlackboard _blackboard;
        private BTSubWndGraphDetails _nodeDetails;
        private BTSubWndGraphSettings _graphSettingsWnd;
        private BTDecoratorSearchWindow _decoSearchWnd;

        // public static Action<string> OnNodeSelected { get; set; }
        public static Action<string, string, string, BTBaseTask> OnNewNodeSelected { get; set; }
        // public Action OnNodeDeleted { get; set; }

        public BTDesignContainer DesignContainer => _serializedDesContainer.targetObject as BTDesignContainer;

        public BTGraphView() : base()
        {
            _blackboard = CreateBlackboard(this, null, null, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new BTSubWndGraphDetails(NODE_INFO_RECT);
            AddElement(_nodeDetails);

            graphViewChanged += OnGraphViewChanged;

            // OnNodeSelected += HandleNodeSelected;
            OnNewNodeSelected += HandleNewNodeSelected;

            BTEditorWindow.OnClose += () =>
            {
                // foreach (var listener in OnNodeSelected.GetInvocationList())
                // {
                //     OnNodeSelected -= (Action<string>) listener;
                // }
                foreach (var listener in OnNewNodeSelected.GetInvocationList())
                {
                    OnNewNodeSelected -= (Action<string, string, string, BTBaseTask>) listener;
                }
            };
        }

        public BTGraphView(BTDesignContainer designContainer) : base()
        {
            _blackboard = CreateBlackboard(this, designContainer.Blackboard, designContainer, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new BTSubWndGraphDetails(NODE_INFO_RECT);
            Add(_nodeDetails);
            Init(designContainer);
            _decoSearchWnd = CreateDecoSearchWindow();

            graphViewChanged += OnGraphViewChanged;
            OnNewNodeSelected += HandleNewNodeSelected;

            BTEditorWindow.OnClose += () =>
            {
                foreach (var listener in OnNewNodeSelected.GetInvocationList())
                {
                    OnNewNodeSelected -= (Action<string, string, string, BTBaseTask>) listener;
                }
            };  
        }

        public void Init(BTDesignContainer designContainer)
        {
            _serializedDesContainer = new SerializedObject(designContainer);

            if (designContainer.NodeDataList == null || designContainer.NodeDataList.Count == 0)
            {
                var initParams = new BTGraphInitParamsNode() { pos = DEFAULT_ROOT_SPAWN_POS, blackboard = _blackboard };
                var root = BTGraphNodeFactory.CreateGraphNode(BTNodeType.Root, initParams);
                AddNode(root, DEFAULT_ROOT_SPAWN_POS);
                return;
            }

            var linkDataList = new List<BTLinkData>(designContainer.NodeDataList.Count);
            var nodeDict = new Dictionary<string, Node>(designContainer.NodeDataList.Count);

            System.Func<BTNodeType, string> GetGraphNodeTypeName = nodeType =>
            {
                switch (nodeType)
                {
                    case BTNodeType.Root:
                        return nameof(BTGraphRoot);
                    case BTNodeType.Selector:
                        return nameof(BTGraphSelector);
                    case BTNodeType.Sequencer:
                        return nameof(BTGraphSequencer);
                    default:
                        return nameof(BTGraphRoot);
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
                    icon = BTGlobalSettings.Instance.GetIcon(GetGraphNodeTypeName(nodeData.NodeType)),
                    OpenDecoSearchWindow = OpenDecoSearchWnd
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
                    icon = BTGlobalSettings.Instance.GetIcon(taskData.Task.GetType().Name),
                    OpenDecoSearchWindow = OpenDecoSearchWnd
                };

                var node = BTGraphNodeFactory.CreateGraphNodeLeaf(initParams);
                return node;
            };

            var nodeDataList = new BTExecListBuilder<BTSerializableNodeDataBase, BTGraphNodeBase>()
                .OnObjectCreate((node, parentGuid) =>
                {
                    if (DesignContainer.TryGetDecorators(node.Guid, out List<BTSerializableDecoData> decorators))
                    {
                        node.InitDecorators(decorators);
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
                    var pos = new Vector2(node.x + node.LabelPosX, node.y);
                    var orderLb = new BTGraphOrderLabel(pos, idx);
                    AddElement(orderLb);
                    node.OrderLabel = orderLb;
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
        }
        
        private BTDecoratorSearchWindow CreateDecoSearchWindow()
        {
            var wnd = ScriptableObject.CreateInstance<BTDecoratorSearchWindow>();
            wnd.Init();
            return wnd;
        }

        private void OpenDecoSearchWnd(string decorateeGuid, Vector2 pos, Action<string, Texture2D> onEntrySelectCb)
        {
            _decoSearchWnd.SetEntrySelectCallback(type =>
            {
                BTBaseTask decoSO = BTGlobalSettings.Instance.PlaygroundMode 
                    ? DesignContainer.CreateDummyTask(type)
                    : DesignContainer.GetOrCreateTask(type);
                onEntrySelectCb(decoSO.Name, BTGlobalSettings.Instance.GetIcon(type));
                string decoGuid = System.Guid.NewGuid().ToString();
                decoSO.SavePropData(decoGuid, Activator.CreateInstance(decoSO.PropertyType));
                DesignContainer.AddDecorator(decorateeGuid, new BTSerializableDecoData(decoGuid, decoSO.Name, decoSO));
            });
            SearchWindow.Open(new SearchWindowContext(pos), _decoSearchWnd);
        }

        private void HandleNewNodeSelected(string guid, string name, string desc, BTBaseTask task)
        {
            _nodeDetails.ShowNodeInfo(name, desc);
            
            if (task != null)
            {
                _nodeDetails.DrawTaskProperties(task.LoadPropValue(guid), task.PropertyType, _blackboard);
            }
            else
            {
                _nodeDetails.ClearTaskPropsContent();
            }
        }

        private void HandleNodeSelected(string guid)
        {
            System.Func<SerializedProperty, bool> FindNodeDetails = nodeDataList =>
            {
                foreach (SerializedProperty node in nodeDataList)
                {
                    var graphData = node.FindPropertyRelative("_graphData");

                    if (graphData.FindPropertyRelative("_guid").stringValue == guid)
                    {
                        var name = graphData.FindPropertyRelative("_name").stringValue;
                        var desc = graphData.FindPropertyRelative("_description").stringValue;
                        _nodeDetails.ShowNodeInfo(name, desc);
                        return true;
                    }
                }

                return false;
            };

            var isFound = FindNodeDetails(_serializedDesContainer.FindProperty("_nodeDataList"));

            if (isFound)
            {
                return;
            }

            System.Func<SerializedProperty, string, (string, string, SerializedProperty)> MapNodeDataList = 
                (nodeDataList, nodeGuid) =>
                {
                    foreach (SerializedProperty node in nodeDataList)
                    {
                        var graphData = node.FindPropertyRelative("_graphData");

                        if (graphData.FindPropertyRelative("_guid").stringValue != nodeGuid)
                        {
                            continue;
                        }

                        var name = graphData.FindPropertyRelative("_name").stringValue;
                        var desc = graphData.FindPropertyRelative("_description").stringValue;
                        // var taskEntries = node
                        //                     .FindPropertyRelative("_task")
                        //                     .FindPropertyRelative("_propMap")
                                            // .FindPropertyRelative("_entries");
                        
                        foreach (SerializedProperty prop in node.FindPropertyRelative("_task"))
                        {
                            Debug.Log(prop.displayName);
                        }

                        // foreach (SerializedProperty entry in taskEntries)
                        // {
                        //     if (entry.stringValue != nodeGuid)
                        //     {
                        //         continue;
                        //     }

                        //     var prop = entry.FindPropertyRelative("Value");
                        //     return (name, desc, entry);
                        // }
                    }

                    return (string.Empty, string.Empty, null);
                };

            System.Func<List<BTSerializableTaskData>, string, (string, string, BTBaseTask)> FindTaskDetails = 
                (taskDataList, nodeGuid) =>
                {
                    foreach (var taskData in taskDataList)
                    {
                        if (taskData.Guid != nodeGuid)
                        {
                            continue;
                        }

                        return (taskData.Name, taskData.Description, taskData.Task);
                    }

                    return (string.Empty, string.Empty, null);
                };

            // FindNodeDetails(_serializedDesContainer.FindProperty("_taskDataList"));
            // var (name, desc, prop) = MapNodeDataList(_serializedDesContainer.FindProperty("_taskDataList"), guid);
            var (name, desc, task) = FindTaskDetails(DesignContainer.TaskDataList, guid);
            _nodeDetails.ShowNodeInfo(name, desc);
            _nodeDetails.DrawTaskProperties(task.LoadPropValue(guid), task.PropertyType, _blackboard);
        }

        private GraphBlackboard CreateBlackboard(
            BTGraphView graphView, 
            Blackboard runtimeBlackboard, 
            ScriptableObject BBContainer,
            string title, 
            Rect rect)
        {
            var blackboard = new GraphBlackboard(runtimeBlackboard, BBContainer, graphView) { title = title, scrollable = true };
            blackboard.SetPosition(rect);      
            return blackboard;
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
                        (element as IBTSerializableNode).OnDelete(DesignContainer);
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
                        var newPos = element.GetPosition();
                        node.OnMove(DesignContainer, graphViewChange.moveDelta);
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
                    outputNode.OnConnect(DesignContainer, inputNode.Guid);
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
            var labelPos = new Vector2(pos.x + convertedNode.LabelPosX, pos.y);
            var orderLb = new BTGraphOrderLabel(pos, 0);
            AddElement(orderLb);
            convertedNode.OrderLabel = orderLb;
            convertedNode.OrderLabel.visible = false;
            convertedNode.OnCreate(DesignContainer, pos);
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
            AddElement(_graphSettingsWnd);
        }
    }
}
