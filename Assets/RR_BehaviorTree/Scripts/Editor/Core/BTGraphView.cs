using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

namespace RR.AI.BehaviorTree
{
    public class BTGraphView : AbstractGraphView
    {
        private class NodeDetails : GraphElement
        {
            private TextField _nameField, _descField;
            private VisualElement _taskPropContainer;

            public NodeDetails(UnityEngine.Rect rect)
            {
                style.backgroundColor = RR.Utils.ColorExtension.Create(96f);
                SetPosition(rect);
                
                var titleContainer = CreateTitle();
                Add(titleContainer);

                // var (name, desc) = selectedNode == null 
                //     ? (string.Empty, string.Empty) 
                //     : (selectedNode.FindPropertyRelative("_name").stringValue, selectedNode.FindPropertyRelative("_description").stringValue);
                var generalContainer = CreateGeneralContainer();
                Add(generalContainer);

                _taskPropContainer = CreateTaskPropContainer();
                Add(_taskPropContainer);
            }

            private VisualElement CreateTitle()
            {
                var container = new VisualElement();
                container.style.alignItems = Align.Center;
                container.style.marginBottom = 5f;

                var label = new Label("Details");
                label.style.fontSize = 14;
                label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;

                container.Add(label);

                return container;
            }

            private VisualElement CreateContainerBase(string labelText, VisualElement content)
            {
                var container = new VisualElement();
                container.style.backgroundColor = RR.Utils.ColorExtension.Create(62f);

                var label = new Label(labelText);
                label.style.backgroundColor = RR.Utils.ColorExtension.Create(30f);
                label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                label.style.paddingLeft = 5f;
                label.style.paddingTop = 2f;
                label.style.paddingBottom = 2f;

                content.style.paddingTop = 2f;
                content.style.paddingBottom = 2f;

                container.Add(label);
                container.Add(content);

                return container;
            }

            private VisualElement CreateGeneralContainer()
            {
                var container = new VisualElement();
                _nameField = new TextField("Name");
                _nameField.labelElement.style.marginRight = -70;
                _descField = new TextField("Description") { multiline = true };
                _descField.labelElement.style.marginRight = -70;
                _descField.style.whiteSpace = WhiteSpace.Normal;

                container.Add(_nameField);
                container.Add(_descField);

                return CreateContainerBase("General", container);
            }
        
            private VisualElement CreateTaskPropContainer()
            {
                var container = new VisualElement();
                return CreateContainerBase("Properties", container);
            }

            public void ShowNodeInfo(string name, string desc)
            {
                _nameField.value = name;
                _descField.value = desc;
            }

            public void ShowTaskProp(SerializedProperty prop)
            {
                _taskPropContainer.Clear();

                if (prop == null)
                {
                    return;
                }

                foreach (SerializedProperty propField in prop)
                {
                    var UIField = new UnityEditor.UIElements.PropertyField(propField, propField.displayName);
                    _taskPropContainer.Add(UIField);
                }
            }
        }

        private readonly UnityEngine.Vector2 DEFAULT_ROOT_SPAWN_POS = new UnityEngine.Vector2(400f, 480f);
        private readonly UnityEngine.Rect NODE_INFO_RECT = new UnityEngine.Rect(10, 30, 320, 560);
        private readonly UnityEngine.Rect BB_RECT = new UnityEngine.Rect(10, 30 + 560 + 10, 320, 400);

        private SerializedObject _serializedDesContainer;
        private GraphBlackboard _blackboard;
        private NodeDetails _nodeDetails;

        public static System.Action<string> OnNodeSelected { get; set; }
        public System.Action OnNodeDeleted { get; set; }

        public BTDesignContainer DesignContainer => _serializedDesContainer.targetObject as BTDesignContainer;

        public BTGraphView() : base()
        {
            _blackboard = CreateBlackboard(this, null, null, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new NodeDetails(NODE_INFO_RECT);
            Add(_nodeDetails);

            graphViewChanged += OnGraphViewChanged;

            OnNodeSelected += HandleNodeSelected;

            BTEditorWindow.OnClose += () =>
            {
                foreach (var listener in OnNodeSelected.GetInvocationList())
                {
                    OnNodeSelected -= (System.Action<string>) listener;
                }
            };
        }

        public BTGraphView(BTDesignContainer designContainer) : base()
        {
            _blackboard = CreateBlackboard(this, designContainer.Blackboard, designContainer, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new NodeDetails(NODE_INFO_RECT);
            Add(_nodeDetails);
            Init(designContainer);
            graphViewChanged += OnGraphViewChanged;       
        }

        public void Init(BTDesignContainer designContainer)
        {
            _serializedDesContainer = new SerializedObject(designContainer);
            OnNodeDeleted = delegate {};

            if (designContainer.NodeDataList == null || designContainer.NodeDataList.Count == 0)
            {
                var root = BTGraphNodeFactory.CreateGraphNode(BTNodeType.Root, DEFAULT_ROOT_SPAWN_POS, _blackboard);
                AddElement(root);
                return;
            }

            var linkDataList = new List<BTLinkData>(designContainer.NodeDataList.Count);
            var nodeDict = new Dictionary<string, Node>(designContainer.NodeDataList.Count);

            designContainer.NodeDataList.ForEach(nodeData => 
            {
                var node = BTGraphNodeFactory.CreateGraphNode(
                    nodeData.NodeType, nodeData.Position, _blackboard, nodeData.Name, nodeData.Description, nodeData.Guid);
                
                // var badge = IconBadge.CreateComment("0");
                // badge.badgeText = "0";
                // badge.distance = 0;
                // node.titleContainer.Add(badge);

                nodeDict.Add(nodeData.Guid, node);
                
                if (!string.IsNullOrEmpty(nodeData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = nodeData.ParentGuid, endGuid = nodeData.Guid });
                }

                AddElement(node);
            });

            designContainer.TaskDataList.ForEach(taskData => 
            {
                var node = BTGraphNodeFactory.CreateGraphNodeLeaf(
                    taskData.Task, taskData.Position, _blackboard, taskData.Name, taskData.Description, taskData.Guid);

                nodeDict.Add(taskData.Guid, node);
                
                if (!string.IsNullOrEmpty(taskData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = taskData.ParentGuid, endGuid = taskData.Guid });
                }

                AddElement(node);
            });

            linkDataList.ForEach(linkData =>
            {
                var (parent, child) = (nodeDict[linkData.startGuid], nodeDict[linkData.endGuid]);
                var edge = (child.inputContainer[0] as Port).ConnectTo(parent.outputContainer[0] as Port);
                AddElement(edge);
            });
        }

        private void HandleNodeSelected(string guid)
        {
            // UnityEngine.Debug.Log(guid);

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
                            UnityEngine.Debug.Log(prop.displayName);
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

            // FindNodeDetails(_serializedDesContainer.FindProperty("_taskDataList"));
            var (name, desc, prop) = MapNodeDataList(_serializedDesContainer.FindProperty("_taskDataList"), guid);

            _nodeDetails.ShowNodeInfo(name, desc);
        }

        private GraphBlackboard CreateBlackboard(
            BTGraphView graphView, 
            Blackboard runtimeBlackboard, 
            UnityEngine.ScriptableObject BBContainer,
            string title, 
            UnityEngine.Rect rect)
        {
            var blackboard = new GraphBlackboard(runtimeBlackboard, BBContainer, graphView) { title = title, scrollable = true };
            blackboard.SetPosition(rect);      
            return blackboard;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            var elementsToRemove = graphViewChange.elementsToRemove;
            
            if (elementsToRemove == null)
            {
                return graphViewChange;
            }
            
            foreach (var element in elementsToRemove)
            {
                OnElementDeleted?.Invoke(element);

                if (element is IBTSavable)
                {
                    OnNodeDeleted += (element as IBTSavable).DeleteCallback;
                }
            }

            return graphViewChange;
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

        public void Save()
        {
            OnNodeDeleted?.Invoke();
            OnNodeDeleted = delegate {};
        }
    }
}
