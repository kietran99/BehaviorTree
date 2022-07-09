using System.Collections.Generic;
using System.Linq;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTVisualDebugger
    {
        private BTScheduler _scheduler;
        private List<BTGraphDebugNode> _debugNodes;
        
        private int _lastIdx, _returnIdx;
        private bool _isFirstCapturedTick;

        private bool _shouldOnlyCaptureOneFrame = false;
        private bool _hasCaptured;

        public BTVisualDebugger(BTScheduler scheduler, List<BTGraphNodeBase> graphNodes)
        {
            _scheduler = scheduler;
            _debugNodes = graphNodes.ConvertAll(node => new BTGraphDebugNode(node));
            _lastIdx = 0;
            _returnIdx = -1;
            _isFirstCapturedTick = true;
            _hasCaptured = false;
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _scheduler.TreeEval += OnTreeEval;
            _scheduler.NodeTick += OnNodeTick;
            _scheduler.NodeReturn += OnNodeReturn;
        }

        private void UnregisterCallbacks()
        {
            _scheduler.TreeEval -= OnTreeEval;
            _scheduler.NodeTick -= OnNodeTick;
            _scheduler.NodeReturn -= OnNodeReturn;
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
            // Debug.Log($"OnNodeTick: {nodeIdx}");

            if (_lastIdx == nodeIdx)
            {
                return;
            }

            if (_lastIdx != 0 && _returnIdx != -1)
            {
                bool found = false;
                int curIdx = _lastIdx;

                do
                {
                    var curNode = _debugNodes[curIdx];
                    curNode.Reset();
                    int parentIdx = _debugNodes[curIdx].ParentIdx;

                    if (parentIdx == 0)
                    {
                        break;
                    }

                    var parentNode = _debugNodes[parentIdx];
                    found = parentNode.IsChildIdx(nodeIdx);
                    curIdx = parentIdx;
                } while (!found);

                _returnIdx = -1;
            }

            _lastIdx = nodeIdx;

            // if (_isFirstCapturedTick)
            // {
            //     int parentIdx = _debugNodes[nodeIdx].ParentIdx;
            //     while (parentIdx > 0)
            //     {
            //         // Debug.Log($"Parent {parentIdx}");
            //         _debugNodes[parentIdx].Tick();
            //         parentIdx = _debugNodes[parentIdx].ParentIdx;
            //     } 

            //     _isFirstCapturedTick = false;
            // }

            _debugNodes[nodeIdx].Tick();
        }

        private void OnNodeReturn(int nodeIdx)
        {
            // Debug.Log($"OnNodeReturn: {nodeIdx}");
            
            _returnIdx = nodeIdx;
        }
    }
}
