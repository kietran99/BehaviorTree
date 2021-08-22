using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTLeaf : BTBaseNode
    {
        private BTBaseTask _task;

        public BTLeaf(BTBaseTask task, string guid) : base(guid)
        {
            _task = task;
        }

        public override bool Init(BTBaseNode[] children, GameObject actor, Blackboard blackboard)
        {
            if (children.Length > 0)
            {
                return false;
            }

            _task.Init(actor, blackboard, _guid);

            return true;
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard)
        {
            return _task.Tick(actor, blackboard, _guid);
        }
    }
}