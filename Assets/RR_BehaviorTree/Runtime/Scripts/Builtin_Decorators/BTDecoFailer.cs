using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTDecoFailer : BTBaseDecorator
    {
        public override string Name => "Failer";

        protected override void OnStart(GameObject actor, RuntimeBlackboard blackboard)
        {}

        protected override BTDecoState OnUpdate(GameObject actor, RuntimeBlackboard blackboard) => BTDecoState.FAILURE;
    }
}
