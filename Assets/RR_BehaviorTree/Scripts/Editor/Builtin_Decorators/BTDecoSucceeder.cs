using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTDecoSucceeder : BTBaseDecorator
    {
        public override string Name => "Succeeder";

        protected override void OnStart(GameObject actor, RuntimeBlackboard blackboard)
        {}

        protected override BTDecoState OnUpdate(GameObject actor, RuntimeBlackboard blackboard) => BTDecoState.SUCCESS;
    }
}
