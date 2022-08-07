using UnityEngine;

using System;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField]
        private GameObject _actor = null;

        [SerializeField]
        private BTDesignContainer _designContainer = null;

        private BTScheduler _scheduler;
        private RuntimeBlackboard _runtimeBlackboard;

        public BTDesignContainer DesignContainer => _designContainer;
        public BTScheduler Scheduler => _scheduler;

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
            
            Func<BTNodeType, BTNodeType, int, int, (int successIdx, int failIdx)> CalcIdxPair = 
                (nodeType, parentType, parentIdx, nextSiblingIdx) =>
                {
                    switch (parentType)
                    {
                        case BTNodeType.Selector:
                            return (parentIdx, nextSiblingIdx);
                        case BTNodeType.Sequencer:
                            return (nextSiblingIdx, parentIdx);
                        default:
                            break;
                    }

                    if (nodeType == BTNodeType.Leaf)
                    {
                        (int successIdx, int failIdx) =
                                parentType == BTNodeType.Sequencer
                                    ? (nextSiblingIdx, parentIdx)
                                    : parentType == BTNodeType.Selector
                                        ? (parentIdx, nextSiblingIdx)
                                        : (parentIdx, parentIdx);
                            return (successIdx, failIdx);
                    }

                    return (0, 0);
                };

            Func<string, System.Collections.Generic.IEnumerable<BTRuntimeDecorator>> FindDecorators = (decorateeGuid) =>
            {
                if (_designContainer.TryGetDecorators(decorateeGuid, out var decoDataList))
                {
                    return decoDataList.Select(deco => new BTRuntimeDecorator(deco.guid, deco.decorator));
                };

                return null;
            };

            BTRuntimeNodeBase[] execList = execListBuilder
                .Execute(
                    _designContainer.AsNodeDataBaseList,
                    data => 
                    {
                        bool isTaskNode = data.GetType().Equals(typeof(BTSerializableTaskData));
                        BTNodeType nodeType = isTaskNode ? BTNodeType.Leaf : (data as BTSerializableNodeData).NodeType;
                        BTBaseTask task = isTaskNode ? (data as BTSerializableTaskData).Task : null;
                        return new RuntimeNodeSortWrapper(
                            data.Guid, 
                            nodeType, nodeType == BTNodeType.Root ? BTNodeType.Root : _designContainer.FindParentType(data.ParentGuid), 
                            (int)data.Position.x, (int)data.Position.y, 
                            task);
                    }
                )
                .Select(nodeWrapper => 
                {
                    int parentIdx = execListBuilder.GetNodeIndex(nodeWrapper.ParentGuid);
                    int nextSiblingIdx = execListBuilder.GetNodeIndex(nodeWrapper.NextSiblingGuid, parentIdx);
                    (int successIdx, int failIdx) = CalcIdxPair(nodeWrapper.Type, nodeWrapper.ParentType, parentIdx, nextSiblingIdx);
                    System.Collections.Generic.IEnumerable<BTRuntimeDecorator> decorators = FindDecorators(nodeWrapper.Guid);
                    
                    if (decorators == null)
                    {
                        return new BTRuntimeNodeBase(
                            nodeWrapper.Guid, 
                            successIdx, failIdx, 
                            nodeWrapper.Type, 
                            nodeWrapper.Task,
                            null);
                    }

                    foreach (var deco in decorators)
                    {
                        if (deco.Task.GetType() == typeof(BTDecoFailer))
                        {
                            successIdx = failIdx;
                        }
                        else if (deco.Task.GetType() == typeof(BTDecoSucceeder))
                        {
                            failIdx = successIdx;
                        }
                    }

                    BTRuntimeDecorator[] filteredDecorators = decorators
                        .Where(deco => deco.Task.GetType() != typeof(BTDecoFailer) && deco.Task.GetType() != typeof(BTDecoSucceeder))
                        .ToArray();

                    return new BTRuntimeNodeBase(
                        nodeWrapper.Guid, 
                        successIdx, failIdx, 
                        nodeWrapper.Type, 
                        nodeWrapper.Task,
                        filteredDecorators);
                })
                .ToArray();

            _runtimeBlackboard = _designContainer.Blackboard.CreateRuntimeBlackboard();
            _scheduler = new BTScheduler(execList, _actor, _runtimeBlackboard);
        }

        private class RuntimeNodeSortWrapper : IBTOrderable, IBTIdentifiable
        {
            public string Guid { get; }
            public string ParentGuid { get; set; }
            public string NextSiblingGuid { get; set; }
            public BTBaseTask Task { get; }
            public BTNodeType Type { get; }
            public BTNodeType ParentType { get; }
            public int x { get; set; }
            public int y { get; set; }

            public RuntimeNodeSortWrapper(string guid, BTNodeType nodeType, BTNodeType parentType, int x, int y, BTBaseTask task = null)
            {
                Guid = guid;
                Type = nodeType;
                ParentType = parentType;
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
