using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseTask : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract void Init(GameObject actor, UnityEditor.Experimental.GraphView.Blackboard blackboard);
        public abstract void Tick(GameObject actor, UnityEditor.Experimental.GraphView.Blackboard blackboard);
    }
}
