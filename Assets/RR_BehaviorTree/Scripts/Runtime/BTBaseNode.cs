using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseNode
    {
        protected string _guid;

        protected BTBaseNode(string guid)
        {
            _guid = guid;
        }

        public abstract bool Init(BTBaseNode[] children, GameObject actor, Blackboard blackboard);
        public abstract BTNodeState Tick(GameObject actor, Blackboard blackboard);
    }
}
