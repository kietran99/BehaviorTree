using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRoot : BTBaseNode
    {
        private BTBaseNode _child;

        public BTRoot(string guid) : base(guid)
        {}

        public override bool Init(BTBaseNode[] children, GameObject actor, Blackboard blackboard)
        {
            if (children.Length != 1)
            {
                return false;
            }
            
            _child = children[0];

            return true;
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard)
        {
            return _child.Tick(actor, blackboard);
        }
    }
}
