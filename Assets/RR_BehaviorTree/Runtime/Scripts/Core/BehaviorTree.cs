using UnityEngine;

using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField]
        private GameObject _actor = null;

        [SerializeField]
        private BTDesignContainer _designContainer = null;

        private BTScheduler<BTRuntimeNodeBase> _scheduler;
        private RuntimeBlackboard _runtimeBlackboard;

        public BTDesignContainer DesignContainer => _designContainer;

        private void Start()
        {
            if (_designContainer.NodeDataList.Count == 0 || _designContainer.TaskDataList.Count == 0)
            {
                Debug.LogError("Invalid Behavior Tree");
                gameObject.SetActive(false);
            }

            var execListBuilder = new BTExecListBuilder<BTSerializableNodeDataBase, RuntimeNodeSortWrapper>()
                .OnObjectCreate((node, parentGuid) =>
                {
                    // if (DesignContainer.TryGetDecorators(node.guid, out List<BTSerializableDecoData> decorators))
                    // {
                    //     node.InitDecorators(decorators);
                    // }

                    node.ParentGuid = parentGuid;
                })
                .OnObjectOrder((node, idx, nextSiblingGuid) =>
                {
                    node.NextSiblingGuid = nextSiblingGuid;
                });
            
            System.Func<BTNodeType, int, int, (int successIdx, int failIdx)> CalcIdxPair = (nodeType, parentIdx, nextSiblingIdx) =>
            {
                switch (nodeType)
                {
                    case BTNodeType.Selector:
                        return (parentIdx, nextSiblingIdx);
                    case BTNodeType.Sequencer:
                        return (nextSiblingIdx, parentIdx);
                    case BTNodeType.Leaf:
                        return (parentIdx, parentIdx);
                    default:
                        break;
                }

                return (0, 0);
            };

            BTRuntimeNodeBase[] execList = execListBuilder
                .Execute(
                    _designContainer.AsNodeDataBaseList,
                    data => 
                    {
                        bool isTaskNode = data.GetType().Equals(typeof(BTSerializableTaskData));
                        BTNodeType nodeType = isTaskNode ? BTNodeType.Leaf : (data as BTSerializableNodeData).NodeType;
                        BTBaseTask task = isTaskNode ? (data as BTSerializableTaskData).Task : null;
                        return new RuntimeNodeSortWrapper(data.Guid, nodeType, (int)data.Position.x, (int)data.Position.y, task);
                    }
                )
                .Select(nodeWrapper => 
                {
                    int parentIdx = execListBuilder.GetNodeIndex(nodeWrapper.ParentGuid);
                    int nextSiblingIdx = execListBuilder.GetNodeIndex(nodeWrapper.NextSiblingGuid, parentIdx);
                    (int successIdx, int failIdx) = CalcIdxPair(nodeWrapper.Type, parentIdx, nextSiblingIdx);
                    return new BTRuntimeNodeBase(nodeWrapper.Guid, successIdx, failIdx, nodeWrapper.Type, nodeWrapper.Task);
                })
                .ToArray();

            _scheduler = new BTScheduler<BTRuntimeNodeBase>(execList, _actor, _runtimeBlackboard);
        }

        private class RuntimeNodeSortWrapper : IBTOrderable, IBTIdentifiable
        {
            private readonly string _guid;

            public string Guid => _guid;
            public string ParentGuid { get; set; }
            public string NextSiblingGuid { get; set; }
            public BTBaseTask Task { get; }
            public BTNodeType Type { get; }
            public int x { get; set; }
            public int y { get; set; }

            public RuntimeNodeSortWrapper(string guid, BTNodeType nodeType, int x, int y, BTBaseTask task = null)
            {
                _guid = guid;
                Type = nodeType;
                this.x = x;
                this.y = y;
                Task = task;
            }
        }

        private void Update()
        {
            _scheduler.Tick();
        }
    }
}
