using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTLeaf : BTBaseNode
    {
        private BTBaseTask _task;

        public override bool IsLeaf => true;

        public BTLeaf(BTBaseTask task, string guid) : base(guid)
        {
            _task = task;
        }

        public override bool Init(BTBaseNode[] children, GameObject actor, RuntimeBlackboard blackboard)
        {
            if (children.Length > 0)
            {
                return false;
            }

            _task.Init(actor, blackboard, _guid);

            return true;
        }

        public override BTNodeState InternalUpdate(GameObject actor, RuntimeBlackboard blackboard, out BTBaseNode runningNode)
        {
            OnTick?.Invoke(_guid);
            var res = _task.Tick(actor, blackboard, _guid);
            runningNode = res == BTNodeState.RUNNING ? this : null;
            return res;
        }

        public override BTNodeState Update(GameObject actor, RuntimeBlackboard blackboard)
        {
            OnTick?.Invoke(_guid);
            var res = _task.Tick(actor, blackboard, _guid);

            return res;
        }
    }
}