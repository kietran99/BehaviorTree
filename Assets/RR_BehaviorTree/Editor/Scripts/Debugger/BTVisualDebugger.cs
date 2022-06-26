using UnityEngine;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTVisualDebugger
    {
        private BTScheduler _scheduler;
        private bool _shouldOnlyCaptureOneFrame = true;
        private bool _hasCaptured;

        public BTVisualDebugger(BTScheduler scheduler)
        {
            _scheduler = scheduler;
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
            Debug.Log("OnTreeEval");

            if (_shouldOnlyCaptureOneFrame)
            {
                if (!_hasCaptured)
                {
                    _hasCaptured = true;
                    return;
                }

                UnregisterCallbacks();
            }
        }

        private void OnNodeTick(int nodeIdx)
        {
            Debug.Log($"OnNodeTick: {nodeIdx}");
        }

        private void OnNodeReturn(int nodeIdx)
        {
            Debug.Log($"OnNodeReturn: {nodeIdx}");
        }
    }
}
