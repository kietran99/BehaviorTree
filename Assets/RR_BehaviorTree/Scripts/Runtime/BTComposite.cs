using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTComposite : BTBaseNode
    {
		protected BTBaseNode[] _children;

        protected BTComposite(string guid) : base(guid)
        {}

        public override bool Init(BTBaseNode[] children, GameObject actor, Blackboard blackboard)
        {
			if (children.Length == 0)
			{
				return false;
			}

			_children = children;

            return true;
        }
    }
}