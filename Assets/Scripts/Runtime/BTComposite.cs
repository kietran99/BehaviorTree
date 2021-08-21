using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTComposite : IBTNode
    {
		protected IBTNode[] _children;

        public bool Init(IBTNode[] children, GameObject actor, Blackboard blackboard)
        {
			if (children.Length == 0)
			{
				return false;
			}

			_children = children;

            return true;
        }

        public abstract BTNodeState Tick(GameObject actor, Blackboard blackboard);
    }
}