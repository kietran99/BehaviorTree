using System.Collections.Generic;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField]
        private GameObject _actor = null;

        [SerializeField]
        private BTDesignContainer _designContainer = null;

        public BTDesignContainer DesignContainer => _designContainer;

        private BTBaseNode _root;

        private void Start()
        {
            if (_designContainer.nodeDataList.Count == 0 || _designContainer.taskDataList.Count == 0)
            {
                Debug.LogError("Invalid Behavior Tree");
                gameObject.SetActive(false);
            }

            var (root, nodeDataList) = Extract(_designContainer);
            _root = root;

            if (_root == null)
            {
                Debug.LogError("No Root node was found");
                gameObject.SetActive(false);
            }

            InitTree(nodeDataList);
        }

        private (BTBaseNode root, BTNodeData[] nodeDataList) Extract(BTDesignContainer designContainer)
        {
            BTBaseNode root = null;
            var nodeDict = new Dictionary<string, (BTBaseNode node, int yPos)>(designContainer.nodeDataList.Count);
            var linkDataList = new List<BTLinkData>(designContainer.nodeDataList.Count);
            var linkDict = new Dictionary<BTBaseNode, List<(BTBaseNode node, int yPos)>>(designContainer.nodeDataList.Count);
            var emptyList = new List<(BTBaseNode node, int yPos)>();
            
            designContainer.nodeDataList.ForEach(nodeData => 
            {
                var node = BTNodeFactory.Create(nodeData.NodeType, nodeData.Guid);

                if (nodeData.NodeType == BTNodeType.Root)
                {
                    root = node;
                }
                
                nodeDict.Add(nodeData.Guid, (node, Mathf.FloorToInt(nodeData.Position.y)));
                linkDict.Add(node, new List<(BTBaseNode node, int yPos)>());
                
                if (!string.IsNullOrEmpty(nodeData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = nodeData.ParentGuid, endGuid = nodeData.Guid });
                }
            });

            designContainer.taskDataList.ForEach(taskData => 
            {
                var node = BTNodeFactory.CreateLeaf(taskData.Task, taskData.Guid);

                nodeDict.Add(taskData.Guid, (node, Mathf.FloorToInt(taskData.Position.y)));
                linkDict.Add(node, emptyList);
                
                if (!string.IsNullOrEmpty(taskData.ParentGuid)) 
                {
                    linkDataList.Add(new BTLinkData() { startGuid = taskData.ParentGuid, endGuid = taskData.Guid });
                }
            });

            var nodePriorityComparer = new NodePriorityComparer();

            linkDataList.ForEach(linkData =>
            {
                var (parent, child) = (nodeDict[linkData.startGuid], nodeDict[linkData.endGuid]);
                var children = linkDict[parent.node];
                children.Add(child);
                children.Sort(nodePriorityComparer);
            });

            return (root, Map(linkDict));
        }

        private class NodePriorityComparer : IComparer<(BTBaseNode node, int yPos)>
        {
            public int Compare((BTBaseNode node, int yPos) x, (BTBaseNode node, int yPos) y) => x.yPos.CompareTo(y.yPos);
        }

        private BTNodeData[] Map(Dictionary<BTBaseNode, List<(BTBaseNode node, int yPos)>> linkDict)
        {
            var nodeDataList = new BTNodeData[linkDict.Count];
            int idx = 0;

            foreach (var entry in linkDict)
            {
                nodeDataList[idx++] = new BTNodeData(entry.Key, Map(entry.Value));
            }

            return nodeDataList;
        }

        private BTBaseNode[] Map(List<(BTBaseNode node, int yPos)> childrenData)
        {
            var childNodes = new BTBaseNode[childrenData.Count];

            for (int i = 0; i < childrenData.Count; i++)
            {
                childNodes[i] = childrenData[i].node;
            }

            return childNodes;
        }

        private void InitTree(BTNodeData[] nodeDataList)
        {
            // Debug.Log(nodeDataList.Length);
            foreach (var data in nodeDataList)
            {
                data.Node.Init(data.Children, _actor, null);
            }
        }

        private void Update()
        {
            _root.Update(_actor, null);
        }
    }
}
