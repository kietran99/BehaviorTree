using UnityEngine;

using System;

using ChoiceStack = System.Collections.Generic.Stack<(int successIdx, int failIdx)>;

namespace RR.AI.BehaviorTree
{
    public class BTScheduler
    {
        private BTRuntimeNodeBase[] _orderedNodes;
        private GameObject _actor;
        private RuntimeBlackboard _blackboard;
        private ChoiceStack _choiceStack;
        private int _runningIdx;

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
            _runningIdx = -1;

            Init(actor, blackboard);

            // for (int i = 0; i < _orderedNodes.Length; i++)
            // {
            //     var node = _orderedNodes[i];
            //     Debug.Log($"{i} ({node.SuccessIdx} {node.FailIdx})");
            // }

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

                if (node.Type == BTNodeType.Leaf)
                {
                    node.Task.Init(_actor, _blackboard, node.Guid);
                }
            }
        }

        public void Tick()
        {
            if (_runningIdx == -1)
            {
                // Debug.Log("Clear Stack");
                _choiceStack.Clear();
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

            if (_runningIdx == -1)
            {
                TreeEval?.Invoke();
            }

            InternalTick(_runningIdx == -1 ? 1 : _runningIdx, _choiceStack);
        }

        private BTNodeState InternalTick(int curIdx, ChoiceStack choiceStack)
        {
            if (curIdx == _orderedNodes.Length)
            {
                Debug.LogWarning($"Incomplete Behavior Tree at node {_orderedNodes[curIdx - 1].Guid}");
                return BTNodeState.Failure;
            }

            BTRuntimeNodeBase curNode = _orderedNodes[curIdx];
            // Debug.Log($"Ticking node {curIdx}");
            NodeTick?.Invoke(curIdx);

            BTRuntimeDecorator[] decorators = curNode.Decorators;

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
                // Debug.Log($"Choice stack push ({curNode.SuccessIdx}, {curNode.FailIdx})");
            }

            return InternalTick(curIdx + 1, choiceStack);
        }

        private BTNodeState OnTickTask(BTRuntimeNodeBase curNode, int curIdx, ChoiceStack choiceStack)
        {
            BTNodeState taskState = curNode.Task.Tick(_actor, _blackboard, curNode.Guid);
            _runningIdx = -1;

            if (taskState == BTNodeState.Running)
            {
                // NodeReturn?.Invoke(curIdx); // May add a RunningNodeReturn event in the future
                _runningIdx = curIdx;
                return BTNodeState.Running;
            }

            NodeReturn?.Invoke(curIdx);

            int contIdx = taskState == BTNodeState.Success ? curNode.SuccessIdx : curNode.FailIdx;
            bool isContIdxNextSibling = contIdx > curIdx;

            if (isContIdxNextSibling)
            {
                return InternalTick(contIdx, choiceStack);
            }

            bool isLowestPriorityNode = choiceStack.Count == 0;
            if (isLowestPriorityNode)
            {
                return taskState;
            }

            int parentIdx = isContIdxNextSibling ? curIdx : contIdx;
            NodeReturn?.Invoke(parentIdx);

            (int parentSuccessIdx, int parentFailIdx) = choiceStack.Pop();
            // Debug.Log($"Choice stack pop ({parentSuccessIdx}, {parentFailIdx})");
            int nextIdx = taskState == BTNodeState.Success ? parentSuccessIdx : parentFailIdx;

            if (nextIdx == 1)
            {
                return taskState;
            }

            return InternalTick(nextIdx, choiceStack);
        }
    
        private bool HasLowerSibling(BTRuntimeNodeBase node, int curIdx) => node.SuccessIdx > curIdx || node.FailIdx > curIdx;
    }
}
