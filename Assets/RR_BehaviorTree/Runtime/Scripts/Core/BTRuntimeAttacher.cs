using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRuntimeAttacher
    {
        public BTRuntimeAttacher(string guid, BTBaseTask task)
        {
            Guid = guid;
            Task = task;
        }

        public string Guid { get; }
        public BTBaseTask Task { get; }

        public void Init(GameObject actor, RuntimeBlackboard blackboard)
            => Task.Init(actor, blackboard, Guid);

        public BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard)
            => Task.Tick(actor, blackboard, Guid);
    }
}
