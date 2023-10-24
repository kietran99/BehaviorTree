using RR.Events;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRuntimeContext
    {
        public BTRuntimeContext(GameObject actor, RuntimeBlackboard blackboard, IEventHub eventHub)
        {
            Actor = actor;
            Blackboard = blackboard;
            EventHub = eventHub;
        }

        public GameObject Actor { get; private set; }
        public RuntimeBlackboard Blackboard { get; private set; }
        public IEventHub EventHub { get; private set; }
    }
}
