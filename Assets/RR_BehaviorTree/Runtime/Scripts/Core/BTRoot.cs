using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRoot : BTBaseNode
    {
        private BTBaseNode _child;
        private BTBaseNode _runningNode;

        public override bool IsLeaf => false;

        public BTRoot(string guid) : base(guid)
        {}

        public override bool Init(BTBaseNode[] children, GameObject actor, RuntimeBlackboard blackboard)
        {
            if (children.Length != 1)
            {
                return false;
            }
            
            _child = children[0];

            return true;
        }

        public override BTNodeState Update(GameObject actor, RuntimeBlackboard blackboard)
        {
            OnRootTick?.Invoke(_guid);
            
            if (_runningNode != null)
            {
                var runningRes = _runningNode.InternalUpdate(actor, blackboard, out var _);
                _runningNode = runningRes == BTNodeState.RUNNING ? _runningNode : null;
                return runningRes;
            }

            var res = _child.InternalUpdate(actor, blackboard, out var runningNode);
            _runningNode = runningNode != null ? runningNode : null;
            return res;
            // return _child.Update(actor, blackboard);
        }

        public override BTNodeState InternalUpdate(GameObject actor, RuntimeBlackboard blackboard, out BTBaseNode runningNodeParent)
        {
            runningNodeParent = null;
            return BTNodeState.FAILURE;
        }
    }
}
