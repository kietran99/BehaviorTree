using UnityEngine;

using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTScheduler<T> where T : BTRuntimeNodeBase
    {
        private T[] _orderedNodes;
        private GameObject _actor;
        private RuntimeBlackboard _blackboard;
        private Stack<(int successIdx, int failIdx)> _choiceStack;
        private bool shouldTickOnce;
        private bool hasTicked;

        public BTScheduler(T[] orderedNodes, GameObject actor, RuntimeBlackboard blackboard)
        {
            _orderedNodes = orderedNodes;
            _actor = actor;
            _blackboard = blackboard;
            _choiceStack = new Stack<(int successIdx, int failIdx)>(_orderedNodes.Length);

            Init(actor, blackboard);

            // foreach (var node in orderedNodes)
            // {
            //     Debug.Log($"{node.Guid} {node.Type} {node.SuccessIdx} {node.FailIdx}");
            // }

            shouldTickOnce = true;
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
            if (shouldTickOnce && !hasTicked)
            {
                InternalTick(1, _choiceStack);
                hasTicked = true;
            }
        }

        private BTNodeState InternalTick(int curIdx, Stack<(int successIdx, int failIdx)> choiceStack)
        {
            if (curIdx == _orderedNodes.Length)
            {
                Debug.LogWarning($"Incomplete Behavior Tree at node {_orderedNodes[curIdx - 1].Guid}");
                return BTNodeState.Failure;
            }

            T curNode = _orderedNodes[curIdx];
            Debug.Log($"Ticking node {curIdx}");

            BTRuntimeDecorator[] decorators = curNode.Decorators;

            if (decorators != null)
            {
                foreach (var deco in decorators)
                {
                    Debug.Log($"Ticking decorator {deco.Task.Name}");
                    BTNodeState decoState = deco.Tick(_actor, _blackboard);

                    if (decoState == BTNodeState.Success)
                    {
                        continue;
                    }

                    // return InternalTick();
                }
            }

            if (curNode.Type != BTNodeType.Leaf)
            {
                bool hasLowerSibling = curNode.SuccessIdx > curIdx || curNode.FailIdx > curIdx;
                if (hasLowerSibling)
                {
                    choiceStack.Push((curNode.SuccessIdx, curNode.FailIdx));
                    // Debug.Log($"Choice stack push ({curNode.SuccessIdx}, {curNode.FailIdx})");
                }

                return InternalTick(curIdx + 1, choiceStack);
            }

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
    }
}
