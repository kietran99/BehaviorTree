using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        private class RuntimeNodeSortWrapper : IBTOrderable, IBTIdentifiable
        {
            public string Guid { get; }
            public string ParentGuid { get; set; }
            public string NextSiblingGuid { get; set; }
            public BTTaskBase Task { get; }
            public BTNodeType Type { get; }
            public BTNodeType ParentType { get; }
            public int x { get; set; }
            public int y { get; set; }

            public RuntimeNodeSortWrapper(string guid, BTNodeType nodeType, BTNodeType parentType, int x, int y, BTTaskBase task = null)
            {
                Guid = guid;
                Type = nodeType;
                ParentType = parentType;
                this.x = x;
                this.y = y;
                Task = task;
            }
        }

        [SerializeField]
        private GameObject _actor = null;

        [SerializeField]
        private BTGraphDesign _designContainer = null;

        [SerializeField]
        private bool _startOnPlay = false;

        private bool _isInitialized;
        private bool _isActive;
        private BTScheduler _scheduler;
        private RuntimeBlackboard _runtimeBlackboard;
        private Events.IEventHub _eventHub;

        public BTGraphDesign DesignContainer => _designContainer;
        public BTScheduler Scheduler => _scheduler;
        public RuntimeBlackboard Blackboard => _runtimeBlackboard;
        public Events.IEventHub RuntimeEventHub => _eventHub;

        private void Start()
        {
            _isInitialized = false;
            _isActive = false;
            if (_startOnPlay)
            {
                InitAndActivate();
            }
        }

        public void InitAndActivate()
        {
            Init();
            Activate();
        }

        public void Init()
        {
            if (_isInitialized)
            {
                return;
            }

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

            Func<string, (IEnumerable<BTRuntimeAttacher>, IEnumerable<BTRuntimeAttacher>)> FindAttachers = (decorateeGuid) =>
            {
                if (!_designContainer.TryGetAttachers(decorateeGuid, out var decorators, out var services))
                {
                    return (null, null);
                };

                return (decorators.Select(deco => new BTRuntimeAttacher(deco.guid, deco.task))
                    , services.Select(service => new BTRuntimeAttacher(service.guid, service.task)));
            };

            BTRuntimeNodeBase[] execList = execListBuilder
                .Execute(
                    _designContainer.AsNodeDataBaseList,
                    data => 
                    {
                        bool isTaskNode = data.GetType().Equals(typeof(BTSerializableTaskData));
                        BTNodeType nodeType = isTaskNode ? BTNodeType.Leaf : (data as BTSerializableNodeData).NodeType;
                        BTTaskBase task = isTaskNode ? (data as BTSerializableTaskData).Task : null;
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
                    (IEnumerable<BTRuntimeAttacher> decorators, IEnumerable<BTRuntimeAttacher> services) = FindAttachers(nodeWrapper.Guid);
                    List<BTRuntimeAttacher> filteredDecorators = null;

                    if (decorators != null)
                    {
                        filteredDecorators = new List<BTRuntimeAttacher>();

                        foreach (var deco in decorators)
                        {
                            Type taskType = deco.Task.GetType();
                            if (taskType == typeof(BTDecoFailer))
                            {
                                successIdx = failIdx;
                                continue;
                            }

                            if (taskType == typeof(BTDecoSucceeder))
                            {
                                failIdx = successIdx;
                                continue;
                            }

                            filteredDecorators.Add(deco);
                        }
                    }

                    var node = new BTRuntimeNodeBase(
                        nodeWrapper.Guid, 
                        successIdx, failIdx, 
                        nodeWrapper.Type, 
                        nodeWrapper.Task);

                    node.Decorators = filteredDecorators == null ? null : filteredDecorators.ToArray();
                    node.Services = services == null ? null : services.ToArray();

                    return node;
                })
                .ToArray();

            _runtimeBlackboard = _designContainer.Blackboard.CreateRuntimeBlackboard();
            _eventHub = new Events.EventHub();
            _scheduler = new BTScheduler(execList, new BTRuntimeContext(_actor, _runtimeBlackboard, _eventHub));

            _isInitialized = true;
            BBEventBroker.Instance.Reset();
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isInitialized || !_isActive)
            {
                return;
            }

            _scheduler.Tick();
        }
    }
}
