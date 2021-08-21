using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRoot : IBTNode
    {
        private IBTNode _child;

        public bool Init(IBTNode[] children, GameObject actor, Blackboard blackboard)
        {
            if (children.Length != 1)
            {
                return false;
            }
            Debug.Log("Init: Root");
            _child = children[0];

            return true;
        }

        public BTNodeState Tick(GameObject actor, Blackboard blackboard)
        {
            return _child.Tick(actor, blackboard);
        }
    }
}
