using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor;

namespace RR.AI.BehaviorTree
{
    public class BTGraphView : AbstractGraphView
    {
        private readonly UnityEngine.Vector2 DEFAULT_ROOT_SPAWN_POS = new UnityEngine.Vector2(400f, 480f);
        private readonly UnityEngine.Rect NODE_INFO_RECT = new UnityEngine.Rect(10, 30, 320, 560);
        private readonly UnityEngine.Rect BB_RECT = new UnityEngine.Rect(10, 30 + 560 + 10, 320, 400);

        private SerializedObject _serializedDesContainer;
        private GraphBlackboard _blackboard;
        private BTGraphDetails _nodeDetails;

        public static System.Action<string> OnNodeSelected { get; set; }
        public static System.Action<string, string, string, BTBaseTask> OnNewNodeSelected { get; set; }
        public System.Action OnNodeDeleted { get; set; }

        public BTDesignContainer DesignContainer => _serializedDesContainer.targetObject as BTDesignContainer;

        public BTGraphView() : base()
        {
            _blackboard = CreateBlackboard(this, null, null, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new BTGraphDetails(NODE_INFO_RECT);
            Add(_nodeDetails);

            graphViewChanged += OnGraphViewChanged;

            // OnNodeSelected += HandleNodeSelected;
            OnNewNodeSelected += HandleNewNodeSelected;

            BTEditorWindow.OnClose += () =>
            {
                // foreach (var listener in OnNodeSelected.GetInvocationList())
                // {
                //     OnNodeSelected -= (System.Action<string>) listener;
                // }
                foreach (var listener in OnNewNodeSelected.GetInvocationList())
                {
                    OnNewNodeSelected -= (System.Action<string, string, string, BTBaseTask>) listener;
                }
            };
        }

        public BTGraphView(BTDesignContainer designContainer) : base()
        {
            _blackboard = CreateBlackboard(this, designContainer.Blackboard, designContainer, "Shared Variables", BB_RECT);
            Add(_blackboard);
            _nodeDetails = new BTGraphDetails(NODE_INFO_RECT);
            Add(_nodeDetails);
            Init(designContainer);
            graphViewChanged += OnGraphViewChanged;

            OnNewNodeSelected += HandleNewNodeSelected;

            BTEditorWindow.OnClose += () =>
            {
                foreach (var listener in OnNewNodeSelected.GetInvocationList())
                {
                    OnNewNodeSelected -= (System.Action<string, string, string, BTBaseTask>) listener;
                }
            };    
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

        private void HandleNewNodeSelected(string guid, string name, string desc, BTBaseTask task)
        {
            _nodeDetails.ShowNodeInfo(name, desc);
            
            if (task != null)
            {
                _nodeDetails.DrawTaskProperties(task.LoadPropData(guid), task.PropertyType, _blackboard);
            }
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
            _nodeDetails.DrawTaskProperties(task.LoadPropData(guid), task.PropertyType, _blackboard);
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
