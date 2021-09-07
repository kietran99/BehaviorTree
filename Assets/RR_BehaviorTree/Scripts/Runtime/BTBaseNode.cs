using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseNode
    {
        public static System.Action<string> OnRootTick { get; set; }
        public static System.Action<string> OnTick { get; set; }

        public string _guid;

        public abstract bool IsLeaf { get; }

        protected BTBaseNode(string guid)
        {
            _guid = guid;
        }

        public abstract bool Init(BTBaseNode[] children, GameObject actor, Blackboard blackboard);
        public abstract BTNodeState Update(GameObject actor, Blackboard blackboard);
        public abstract BTNodeState InternalUpdate(GameObject actor, Blackboard blackboard, out BTBaseNode runningNode);
    }
}
