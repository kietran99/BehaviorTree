using UnityEngine;

using System;

using ChoiceStack = System.Collections.Generic.Stack<(int successIdx, int failIdx)>;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTScheduler
    {
        private class AbortHandler
        {
            private int _activatedIdx;
            private Func<int, BTNodeState> _handler;

            public bool IsResolved { get; private set; }

            public AbortHandler()
            {
                IsResolved = true;
            }

            public void Bind(int activatedIdx, Func<int, BTNodeState> handler)
            {
                IsResolved = false;
                _activatedIdx = activatedIdx;
                _handler = handler;
            }

            public BTNodeState Resolve()
            {
                IsResolved = true;
                return _handler(_activatedIdx);
            }
        }

        private BTRuntimeNodeBase[] _orderedNodes;
        private ChoiceStack _choiceStack;
        private int _runningIdx;
        private Stack<int> _activeServicesStack;
        private Events.IEventHub _eventHub;

        public Action TreeEval;
        public Action<int> NodeTick;
        public Action<int> NodeReturn;
        public Action<int> ServiceEval;
        public Action<int, int> NodeAbort;

        private AbortHandler _abortHandler;

        private bool shouldTickOnce;
        private bool hasTicked;

        public BTScheduler(BTRuntimeNodeBase[] orderedNodes, BTRuntimeContext context)
        {
            _orderedNodes = orderedNodes;
            _choiceStack = new ChoiceStack(_orderedNodes.Length);
            _activeServicesStack = new Stack<int>();
            ResetRunningIdx();
            _abortHandler = new AbortHandler();
            Init(context);

            // LogSetupStats(context.Blackboard);

            shouldTickOnce = false;
            hasTicked = false;
        }

        private void Init(BTRuntimeContext context)
        {
            for (int i = 0; i < _orderedNodes.Length; i++)
            {
                BTRuntimeNodeBase node = _orderedNodes[i];
                if (node.Decorators != null)
                {
                    foreach (var deco in node.Decorators)
                    {
                        deco.Init(context);

                        var decoTask = deco.Task as BTDecoratorBase;

                        if (decoTask != null)
                        {
                            int idx = i;
                            decoTask.AbortTriggered += (observerAborts, hasMetCondition) => OnAbortTrigger(idx, observerAborts, hasMetCondition);
                        }
                    }
                }

                if (node.Services != null)
                {
                    foreach (var service in node.Services)
                    {
                        service.Init(context);
                    }
                }

                if (node.Type == BTNodeType.Leaf)
                {
                    node.Task.Init(context);
                }
            }

            _eventHub = context.EventHub;
        }

        public void Tick()
        {
            // _eventHub.Publisher<SchedulerTickEvent>()?.Invoke(Time.deltaTime);
            bool hasNoRunningNode = !HasRunningNode;

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
                    BTNodeState decoState = deco.Update();
                    // Debug.Log($"Ticking decorator {deco.Task.Name}: {decoState}");
                    if (!_abortHandler.IsResolved)
                    {
                        return _abortHandler.Resolve();
                    }

                    if (decoState == BTNodeState.Success)
                    {
                        continue;
                    }

                    bool isLowestPriorityNode = !HasLowerSibling(curNode, curIdx);

                    if (isLowestPriorityNode)
                    {
                        return BTNodeState.Failure;
                    }

                    NodeReturn?.Invoke(curIdx);
                    return InternalTick(curNode.FailIdx, choiceStack);
                }
            }

            BTRuntimeAttacher[] services = curNode.Services;
            if (services != null && !HasRunningNode)
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
            if (!HasRunningNode)
            {
                curNode.Task.Enter();
            }

            BTNodeState taskState = curNode.Task.Update();
            if (!_abortHandler.IsResolved)
            {
                return _abortHandler.Resolve();
            }

            if (taskState == BTNodeState.Running)
            {
                // NodeReturn?.Invoke(curIdx); // May add a RunningNodeReturn event in the future

                foreach (var idx in _activeServicesStack)
                {
                    var serviceAttachee = _orderedNodes[idx];

                    foreach (var service in serviceAttachee.Services)
                    {
                        service.Update();
                        ServiceEval?.Invoke(idx);

                        if (!_abortHandler.IsResolved)
                        {
                            return _abortHandler.Resolve();
                        }  
                    }
                }

                _runningIdx = curIdx;
                return BTNodeState.Running;
            }
            
            curNode.Task.Exit();
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

            bool hasParentContainService = _activeServicesStack.Count > 0 && parentIdx == _activeServicesStack.Peek();
            if (hasParentContainService)
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

        private void OnAbortTrigger(int idx, ObserverAborts observerAborts, bool hasMetCondition)
        {
            // Debug.Log($"OnAbortTrigger: idx = {idx} observerAborts = {observerAborts} hasMetCondition = {hasMetCondition}");
            BTRuntimeNodeBase triggeredNode = _orderedNodes[idx];
            int triggeredNodeProgressIdx = triggeredNode.ProgressIdx;
            bool isLowerSubTreeRunning = _runningIdx >= triggeredNodeProgressIdx;
            bool isSelfSubTreeRunning = !isLowerSubTreeRunning;

            switch (observerAborts)
            {
                case ObserverAborts.None:
                {
                    return;
                }
                case ObserverAborts.Self:
                {
                    if (hasMetCondition || isLowerSubTreeRunning)
                    {
                        return;
                    }

                    if (HasRunningNode)
                    {
                        _orderedNodes[_runningIdx].Task.Abort();
                    }

                    _abortHandler.Bind(idx, OnObserverAbortsSelf);
                    return;
                }
                case ObserverAborts.LowerPriority:
                {
                    if (!hasMetCondition || isSelfSubTreeRunning)
                    {
                        return;
                    }

                    if (HasRunningNode)
                    {
                        _orderedNodes[_runningIdx].Task.Abort();
                    }
                    _abortHandler.Bind(idx, OnObserverAbortsLowerPriority);
                    return;
                }
                case ObserverAborts.Both:
                {
                    if (!hasMetCondition && isSelfSubTreeRunning)
                    {
                        if (HasRunningNode)
                        {
                            _orderedNodes[_runningIdx].Task.Abort();
                        }
                        _abortHandler.Bind(idx, OnObserverAbortsSelf);
                        return;
                    }

                    if (hasMetCondition && isLowerSubTreeRunning)
                    {
                        if (HasRunningNode)
                        {
                            _orderedNodes[_runningIdx].Task.Abort();
                        }
                        _abortHandler.Bind(idx, OnObserverAbortsLowerPriority);
                        return;
                    }

                    return;
                }
            }
        }

        private BTNodeState OnObserverAbortsSelf(int idx)
        {
            ResetRunningIdx();
            AbortSelfServices();
            int failIdx = _orderedNodes[idx].FailIdx;
            RegenerateChoiceStackTo(failIdx);
            NodeAbort?.Invoke(_runningIdx, failIdx);
            return InternalTick(failIdx, _choiceStack);
        }

        private void AbortSelfServices()
        {

        }

        private BTNodeState OnObserverAbortsLowerPriority(int idx)
        {
            ResetRunningIdx();
            AbortLowerPriorityServices();
            int parentIdx = _orderedNodes[idx].ParentIdx;
            RegenerateChoiceStackTo(parentIdx);
            NodeAbort?.Invoke(_runningIdx, idx);
            return InternalTick(idx, _choiceStack);
        }

        private void AbortLowerPriorityServices()
        {

        }

        private void RegenerateChoiceStackTo(int idx)
        {
            int curIdx = idx;
            var reverseStack = new ChoiceStack();

            while (curIdx > 1)
            {
                BTRuntimeNodeBase curNode = _orderedNodes[idx];
                reverseStack.Push((curNode.SuccessIdx, curNode.FailIdx));
                curIdx = curNode.ParentIdx;
            }

            _choiceStack.Clear();
            
            while (reverseStack.Count > 0)
            {
                _choiceStack.Push(reverseStack.Pop());
            }
        }

        private bool HasLowerSibling(BTRuntimeNodeBase node, int curIdx) => node.SuccessIdx > curIdx || node.FailIdx > curIdx;
    
        private bool HasRunningNode => _runningIdx != -1;

        private void ResetRunningIdx() => _runningIdx = -1;

        private void LogSetupStats(RuntimeBlackboard blackboard)
        {
            for (int i = 0; i < _orderedNodes.Length; i++)
            {
                var node = _orderedNodes[i];
                Debug.Log($"[{i}] - {node.ToString()}");
            }

            blackboard.Log();
        }
    }
}
