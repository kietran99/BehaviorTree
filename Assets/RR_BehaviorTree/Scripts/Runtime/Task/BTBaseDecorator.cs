using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseDecorator<T> : BTBaseTask<T>, IBTBaseDecorator where T : class, new()
    {
        protected abstract void OnStart(GameObject actor, RuntimeBlackboard blackboard, T prop);

        protected abstract BTDecoState OnUpdate(GameObject actor, RuntimeBlackboard blackboard, T prop);

        public sealed override void Init(GameObject actor, RuntimeBlackboard blackboard, T prop)
            => OnStart(actor, blackboard, prop);

        public sealed override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, T prop)
            => OnUpdate(actor, blackboard, prop).ToBTNodeState();
    }

    public interface IBTBaseDecorator {}
}
