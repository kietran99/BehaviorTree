using UnityEngine;

using System.Collections.Generic;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTVisualDebugger
    {
        private BTScheduler _scheduler;
        private List<BTGraphDebugNode> _debugNodes;
        
        private bool _shouldOnlyCaptureOneFrame = false;
        private bool _hasCaptured;

        public BTVisualDebugger(BTScheduler scheduler, List<BTGraphNodeBase> graphNodes)
        {
            _scheduler = scheduler;
            _debugNodes = graphNodes.ConvertAll(node => new BTGraphDebugNode(node));
            _debugNodes.ForEach(node => node.Reset());
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

            _debugNodes.ForEach(node => node.Reset());
        }

        private void OnNodeTick(int nodeIdx)
        {
            Debug.Log($"OnNodeTick: {nodeIdx}");

            _debugNodes[nodeIdx].Tick();
        }

        private void OnNodeReturn(int nodeIdx)
        {
            Debug.Log($"OnNodeReturn: {nodeIdx}");
            
            _debugNodes[nodeIdx].Reset();
        }
    }
}
