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
        private bool shouldTickOnce;
        private bool hasTicked;

        public static Action TreeEval;
        public static Action<string, string> NodeTick;
        public static Action<string> NodeReturn;

        public BTScheduler(BTRuntimeNodeBase[] orderedNodes, GameObject actor, RuntimeBlackboard blackboard)
        {
            _orderedNodes = orderedNodes;
            _actor = actor;
            _blackboard = blackboard;
            _choiceStack = new ChoiceStack(_orderedNodes.Length);

            Init(actor, blackboard);

            // foreach (var node in orderedNodes)
            // {
            //     Debug.Log($"{node.Guid} {node.Type} {node.SuccessIdx} {node.FailIdx}");
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
            _choiceStack.Clear();
            
            if (shouldTickOnce)
            {
                if (!hasTicked)
                {
                    hasTicked = true;
                    InternalTick(1, _choiceStack);
                }

                return;
            }

            InternalTick(1, _choiceStack);
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

            if (taskState == BTNodeState.Running)
            {
                return BTNodeState.Running;
            }

            bool isLowestPriorityNode = (choiceStack.Count == 0) && (curIdx == _orderedNodes.Length - 1);
            if (isLowestPriorityNode)
            {
                return taskState;
            }

            int contIdx = taskState == BTNodeState.Success ? curNode.SuccessIdx : curNode.FailIdx;
            bool isContIdxNextSibling = contIdx > curIdx;

            if (isContIdxNextSibling)
            {
                return InternalTick(contIdx, choiceStack);
            }

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
