using UnityEngine;

using System;

using ChoiceStack = System.Collections.Generic.Stack<(int successIdx, int failIdx)>;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTScheduler
    {
        private BTRuntimeNodeBase[] _orderedNodes;
        private GameObject _actor;
        private RuntimeBlackboard _blackboard;
        private ChoiceStack _choiceStack;
        private int _runningIdx;
        private Stack<int> _activeServicesStack;

        public Action TreeEval;
        public Action<int> NodeTick;
        public Action<int> NodeReturn;

        private bool shouldTickOnce;
        private bool hasTicked;

        public BTScheduler(BTRuntimeNodeBase[] orderedNodes, GameObject actor, RuntimeBlackboard blackboard)
        {
            _orderedNodes = orderedNodes;
            _actor = actor;
            _blackboard = blackboard;
            _choiceStack = new ChoiceStack(_orderedNodes.Length);
            _activeServicesStack = new Stack<int>();
            ResetRunningIdx();

            Init(actor, blackboard);

            LogSetupStats();

            shouldTickOnce = false;
            hasTicked = false;
        }

        private void Init(GameObject actor, RuntimeBlackboard blackboard)
        {
            foreach (var node in _orderedNodes)
            {
                if (node.Decorators != null)
                {
                    foreach (var deco in node.Decorators)
                    {
                        deco.Init(actor, blackboard);
                    }
                }

                if (node.Services != null)
                {
                    foreach (var service in node.Services)
                    {
                        service.Init(actor, blackboard);
                    }
                }

                if (node.Type == BTNodeType.Leaf)
                {
                    node.Task.Init(_actor, _blackboard, node.Guid);
                }
            }
        }

        public void Tick()
        {
            bool hasNoRunningNode = !HasRunningNode();

            if (hasNoRunningNode)
            {
                _choiceStack.Clear();
                _activeServicesStack.Clear();
            }
            
            if (shouldTickOnce)
            {
                if (!hasTicked)
                {
                    hasTicked = true;
                    TreeEval?.Invoke();
                    InternalTick(1, _choiceStack);
                }

                return;
            }

            if (hasNoRunningNode)
            {
                TreeEval?.Invoke();
            }

            int tickIdx = hasNoRunningNode ? 1 : _runningIdx;
            InternalTick(tickIdx, _choiceStack);
        }

        private BTNodeState InternalTick(int curIdx, ChoiceStack choiceStack)
        {
            if (curIdx == _orderedNodes.Length)
            {
                Debug.LogWarning($"Incomplete Behavior Tree at node {_orderedNodes[curIdx - 1].Guid}");
                return BTNodeState.Failure;
            }

            BTRuntimeNodeBase curNode = _orderedNodes[curIdx];
            NodeTick?.Invoke(curIdx);

            BTRuntimeAttacher[] decorators = curNode.Decorators;

            if (decorators != null)
            {
                foreach (var deco in decorators)
                {
                    BTNodeState decoState = deco.Tick(_actor, _blackboard);
                    // Debug.Log($"Ticking decorator {deco.Task.Name}: {decoState}");

                    if (decoState == BTNodeState.Success)
                    {
                        continue;
                    }

                    bool isLowestPriorityNode = HasLowerSibling(curNode, curIdx);
                    return isLowestPriorityNode
                        ? BTNodeState.Failure
                        : InternalTick(curNode.FailIdx, choiceStack);
                }
            }

            BTRuntimeAttacher[] services = curNode.Services;
            if (services != null)
            {
                _activeServicesStack.Push(curIdx);
                // foreach (var service in services)
                // {

                // }
            }

            return curNode.Type == BTNodeType.Leaf
                ? OnTickTask(curNode, curIdx, choiceStack)
                : OnTickComposite(curNode, curIdx, choiceStack);
        }

        private BTNodeState OnTickComposite(BTRuntimeNodeBase curNode, int curIdx, ChoiceStack choiceStack)
        {
            bool hasLowerSibling = HasLowerSibling(curNode, curIdx);
            if (hasLowerSibling)
            {
                choiceStack.Push((curNode.SuccessIdx, curNode.FailIdx));
                // Debug.Log($"Choice stack push ({curNode.SuccessIdx} : {curNode.FailIdx})");
            }

            return InternalTick(curIdx + 1, choiceStack);
        }

        private BTNodeState OnTickTask(BTRuntimeNodeBase curNode, int curIdx, ChoiceStack choiceStack)
        {
            BTNodeState taskState = curNode.Task.Tick(_actor, _blackboard, curNode.Guid);

            if (taskState == BTNodeState.Running)
            {
                // NodeReturn?.Invoke(curIdx); // May add a RunningNodeReturn event in the future

                foreach (var idx in _activeServicesStack)
                {
                    var serviceAttachee = _orderedNodes[idx];

                    foreach (var service in serviceAttachee.Services)
                    {
                        service.Tick(_actor, _blackboard);
                    }
                }

                _runningIdx = curIdx;
                return BTNodeState.Running;
            }

            bool finishedRunning = HasRunningNode();
            
            ResetRunningIdx();
            NodeReturn?.Invoke(curIdx);

            int contIdx = taskState == BTNodeState.Success ? curNode.SuccessIdx : curNode.FailIdx;
            bool shouldTickNextSibling = contIdx > curIdx;

            if (shouldTickNextSibling)
            {
                return InternalTick(contIdx, choiceStack);
            }

            int parentIdx = contIdx;
            NodeReturn?.Invoke(parentIdx);

            if (_activeServicesStack.Count > 0 && parentIdx == _activeServicesStack.Peek())
            {
                int popIdx = _activeServicesStack.Pop();
                // Debug.Log($"Services stack pop {popIdx}");
            }

            bool isLowestPriorityNode = choiceStack.Count == 0;
            if (isLowestPriorityNode)
            {
                return taskState;
            }

            (int parentSuccessIdx, int parentFailIdx) = choiceStack.Pop();
            // Debug.Log($"Choice stack pop ({parentSuccessIdx} : {parentFailIdx})");
            int nextIdx = taskState == BTNodeState.Success ? parentSuccessIdx : parentFailIdx;

            while (_activeServicesStack.Count > 0)
            {
                int topIdx = _activeServicesStack.Peek();
                int topNodeProgressIdx = _orderedNodes[topIdx].ProgressIdx;
                if (topNodeProgressIdx > nextIdx || topIdx == 1) // Services of node #1 are always active until BT reevaluates
                {
                    break;
                }

                _activeServicesStack.Pop();
                // Debug.Log($"Services stack pop {topIdx}");
            }

            if (nextIdx == 1)
            {
                return taskState;
            }

            return InternalTick(nextIdx, choiceStack);
        }
    
        private bool HasLowerSibling(BTRuntimeNodeBase node, int curIdx) => node.SuccessIdx > curIdx || node.FailIdx > curIdx;
    
        private bool HasRunningNode() => _runningIdx != -1;

        private void ResetRunningIdx() => _runningIdx = -1;

        private void LogSetupStats()
        {
            for (int i = 0; i < _orderedNodes.Length; i++)
            {
                var node = _orderedNodes[i];
                Debug.Log($"[{i}] - {node.ToString()}");
            }

            _blackboard.Log();
        }
    }
}
