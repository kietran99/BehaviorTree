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
        private bool tickOnce;
        public BTScheduler(T[] orderedNodes, GameObject actor, RuntimeBlackboard blackboard)
        {
            _orderedNodes = orderedNodes;
            _actor = actor;
            _blackboard = blackboard;
            _choiceStack = new Stack<(int successIdx, int failIdx)>(_orderedNodes.Length);

            foreach (var node in orderedNodes)
            {
                Debug.Log($"{node.Guid} {node.Type} {node.SuccessIdx} {node.FailIdx}");
            }

            tickOnce = false;
        }

        public void Tick()
        {
            _choiceStack.Clear();
            if (!tickOnce)
            {
                InternalTick(1, _choiceStack);
                // tickOnce = true;
            }
        }

        private BTNodeState InternalTick(int curIdx, Stack<(int successIdx, int failIdx)> choiceStack)
        {
            if (curIdx == _orderedNodes.Length)
            {
                Debug.LogWarning($"Incomplete Behavior Tree at node {_orderedNodes[curIdx - 1].Guid}");
                return BTNodeState.FAILURE;
            }

            T curNode = _orderedNodes[curIdx];

            if (curNode.Type != BTNodeType.Leaf)
            {
                if (curNode.SuccessIdx != curNode.FailIdx) // if not the lowest-priority node
                {
                    choiceStack.Push((curNode.SuccessIdx, curNode.FailIdx));
                }

                return InternalTick(curIdx + 1, choiceStack);
            }

            BTNodeState taskState = curNode.Task.Tick(_actor, _blackboard, curNode.Guid);

            if (taskState == BTNodeState.RUNNING)
            {
                return BTNodeState.RUNNING;
            }

            if (choiceStack.Count == 0)
            {
                return taskState;
            }

            var choice = choiceStack.Pop();
            var nextIdx = taskState == BTNodeState.SUCCESS ? choice.successIdx : choice.failIdx;
            return InternalTick(nextIdx, choiceStack);
        }
    }
}
