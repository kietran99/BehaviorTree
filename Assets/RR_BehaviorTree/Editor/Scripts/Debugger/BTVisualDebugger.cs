using System.Collections.Generic;
using System.Linq;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTVisualDebugger
    {
        private BTScheduler _scheduler;
        private List<BTGraphDebugNode> _debugNodes;
        
        private int _lastIdx, _returnIdx;
        private bool _isFirstCapturedTick, _isInitFrame; // Starts exec logic on the 2nd frame

        private bool _shouldOnlyCaptureOneFrame = false;
        private bool _hasCaptured;

        public BTVisualDebugger(BTScheduler scheduler, List<BTGraphNodeBase> graphNodes)
        {
            _scheduler = scheduler;
            _debugNodes = graphNodes.ConvertAll(node => new BTGraphDebugNode(node));
            _lastIdx = 0;
            _returnIdx = -1;
            _isInitFrame = true;
            _isFirstCapturedTick = true;
            _hasCaptured = false;
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _scheduler.TreeEval += OnTreeEval;
            _scheduler.NodeTick += OnNodeTick;
            _scheduler.NodeReturn += OnNodeReturn;
            _scheduler.NodeAbort += OnNodeAbort;
        }

        private void UnregisterCallbacks()
        {
            _scheduler.TreeEval -= OnTreeEval;
            _scheduler.NodeTick -= OnNodeTick;
            _scheduler.NodeReturn -= OnNodeReturn;
            _scheduler.NodeAbort -= OnNodeAbort;
        }

        private void OnTreeEval()
        {
            // Debug.Log("OnTreeEval");

            if (_shouldOnlyCaptureOneFrame)
            {
                if (!_hasCaptured)
                {
                    _hasCaptured = true;
                    return;
                }

                UnregisterCallbacks();
            }

            ResetAllNodes();
        }

        private void ResetAllNodes()
        {
            var allNodesExceptRoot = _debugNodes.Skip(1);
            foreach (var node in allNodesExceptRoot)
            {
                node.Reset();
            }
        }

        private void OnNodeTick(int nodeIdx)
        {
            // UnityEngine.Debug.Log($"OnNodeTick: {nodeIdx}");
            if (_isInitFrame || _isFirstCapturedTick)
            {
                if (_isInitFrame)
                {
                    _isInitFrame = false;
                    return;
                }

                int parentIdx = _debugNodes[nodeIdx].ParentIdx;

                while (parentIdx > 0)
                {
                    _debugNodes[parentIdx].Tick();
                    parentIdx = _debugNodes[parentIdx].ParentIdx;
                }

                _debugNodes[nodeIdx].Tick();
                _isFirstCapturedTick = false;
            }

            if (_lastIdx == nodeIdx)
            {
                return;
            }

            if (_lastIdx != 0 && _returnIdx != -1)
            {
                ResetInactiveNodes(nodeIdx);
                _returnIdx = -1;
            }

            _lastIdx = nodeIdx;

            _debugNodes[nodeIdx].Tick();
        }

        private void ResetInactiveNodes(int activeNodeIdx)
        {
            bool found = false;
            int curIdx = _lastIdx;

            do
            {
                var curNode = _debugNodes[curIdx];
                curNode.Reset();
                int parentIdx = curNode.ParentIdx;

                if (parentIdx == 0)
                {
                    break;
                }

                var parentNode = _debugNodes[parentIdx];
                found = parentNode.IsChildIdx(activeNodeIdx);
                curIdx = parentIdx;
            } while (!found);
        }

        private void OnNodeReturn(int nodeIdx)
        {
            UnityEngine.Debug.Log($"OnNodeReturn: {nodeIdx}");
            
            _returnIdx = nodeIdx;
        }

        private void OnNodeAbort(int abortedIdx, int triggeredIdx)
        {
            // UnityEngine.Debug.Log($"OnNodeAbort: {nodeIdx}");
            ResetAllNodes();
            GenDebugVisualUpTo(triggeredIdx);
        }

        private void GenDebugVisualUpTo(int nodeIdx)
        {
            int nextIdx = nodeIdx;
            while (nextIdx > 0)
            {
                BTGraphDebugNode curNode = _debugNodes[nextIdx];
                curNode.Tick();
                nextIdx = curNode.ParentIdx;
            }
        }
    }
}
