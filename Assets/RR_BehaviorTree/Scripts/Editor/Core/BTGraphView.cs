using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTGraphView : AbstractGraphView
    {
        private UnityEngine.Vector2 defaultRootSpawnPos = new UnityEngine.Vector2(100f, 300f);
        private readonly UnityEngine.Rect BB_RECT = new UnityEngine.Rect(10, 30, 250, 500);

        private GraphBlackboard _blackboard;

        public System.Action OnNodeDeleted { get; set; }

        public BTGraphView() : base()
        {
            _blackboard = CreateBlackboard(this, null, null, "Shared Variables", BB_RECT);
            Add(_blackboard);
            graphViewChanged += OnGraphViewChanged;
        }

        public BTGraphView(BTDesignContainer designContainer) : base()
        {
            UpdateView(designContainer);
            _blackboard = CreateBlackboard(this, designContainer.Blackboard, designContainer, "Shared Variables", BB_RECT);
            Add(_blackboard);
            graphViewChanged += OnGraphViewChanged;       
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

        public void OnGOSelectionChanged(BTDesignContainer designContainer)
        {          
            if (designContainer == null)
            {
                ClearNodesAndEdges();
                _blackboard.visible = false;
                return;
            }

            ClearNodesAndEdges();
            _blackboard.visible = true;
            _blackboard.OnGOSelectionChanged(designContainer.Blackboard, designContainer);
            UpdateView(designContainer);
        }

        protected override void ClearNodesAndEdges()
        {
            nodes.ForEach(node => (node as IBTGraphNode).OnRemove());
            base.ClearNodesAndEdges();
        }

        public void UpdateView(BTDesignContainer designContainer)
        {
            OnNodeDeleted = delegate {};

            if (designContainer.nodeDataList == null || designContainer.nodeDataList.Count == 0)
            {
                var root = BTGraphNodeFactory.CreateGraphNode(BTNodeType.Root, defaultRootSpawnPos, _blackboard);
                AddElement(root);
                return;
            }

            var linkDataList = new List<BTLinkData>(designContainer.nodeDataList.Count);
            var nodeDict = new Dictionary<string, Node>(designContainer.nodeDataList.Count);

            designContainer.nodeDataList.ForEach(nodeData => 
            {
                var node = BTGraphNodeFactory.CreateGraphNode(nodeData.NodeType, nodeData.Position, _blackboard, nodeData.Guid);
                
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

            designContainer.taskDataList.ForEach(taskData => 
            {
                var node = BTGraphNodeFactory.CreateGraphNodeLeaf(taskData.Task, taskData.Position, _blackboard, taskData.Guid);

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

        public override UnityEditor.Experimental.GraphView.Blackboard GetBlackboard() => _blackboard;

        public void Save()
        {
            OnNodeDeleted?.Invoke();
            OnNodeDeleted = delegate {};
        }
    }
}
