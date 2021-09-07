using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

namespace RR.AI.BehaviorTree
{
    public class BTGraphView : AbstractGraphView
    {
        private UnityEngine.Vector2 defaultRootSpawnPos = new UnityEngine.Vector2(100f, 300f);

        public System.Action OnNodeDeleted { get; set; }

        public BTGraphView() : base()
        {
            graphViewChanged += OnGraphViewChanged;
        }

        public BTGraphView(BTDesignContainer designContainer) : base()
        {
            UpdateView(designContainer);
            graphViewChanged += OnGraphViewChanged;       
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
                return;
            }

            ClearNodesAndEdges();
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
                var root = BTGraphNodeFactory.CreateNode(BTNodeType.Root, defaultRootSpawnPos);
                AddElement(root);
                return;
            }

            var linkDataList = new List<BTLinkData>(designContainer.nodeDataList.Count);
            var nodeDict = new Dictionary<string, Node>(designContainer.nodeDataList.Count);

            designContainer.nodeDataList.ForEach(nodeData => 
            {
                var node = BTGraphNodeFactory.CreateNode(nodeData.NodeType, nodeData.Position, nodeData.Guid);
                
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
                var node = BTGraphNodeFactory.CreateNode(taskData.Task, taskData.Position, taskData.Guid);

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

        public void OnGraphSaved()
        {
            OnNodeDeleted?.Invoke();
            OnNodeDeleted = delegate {};
        }
    }
}
