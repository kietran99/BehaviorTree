using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTLeaf : IBTNode
    {
        private BTBaseTask _task;

        public BTLeaf(BTBaseTask task)
        {
            _task = task;
        }

        public bool Init(IBTNode[] children, GameObject actor, Blackboard blackboard)
        {
            if (children.Length > 0)
            {
                return false;
            }

            _task.Init(actor, blackboard);

            return true;
        }

        public BTNodeState Tick(GameObject actor, Blackboard blackboard)
        {
            return _task.Tick(actor, blackboard);
        }
    }
}