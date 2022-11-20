using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public interface IBTService {}

    public abstract class BTServiceBase<T> : BTBaseTask<T>, IBTService where T : BTPropServiceBase, new()
    {
        protected abstract void OnStart(GameObject actor, RuntimeBlackboard blackboard, T prop);
        protected abstract void OnUpdate(GameObject actor, RuntimeBlackboard blackboard, T prop);

        public sealed override void Init(GameObject actor, RuntimeBlackboard blackboard, T prop)
        {
            prop.Reset();
            OnStart(actor, blackboard, prop);
        }

        public sealed override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, T prop)
        {
            float deltaTime = Time.deltaTime;
            bool finishedInterval = prop.Tick(deltaTime);
            if (finishedInterval)
            {
                OnUpdate(actor, blackboard, prop);
            }
            
            return BTNodeState.Success;
        }

        public override void OnTreeEval(GameObject actor, RuntimeBlackboard blackboard, T prop)
        {
            prop.Reset();
        }
    }
}
