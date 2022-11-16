using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public interface IBTService {}

    public abstract class BTPropServiceBase
    {
        public float TickInterval;
        public float RandomDeviation = 0.0f;

        private float counter = 0.0f;
        private float minInterval, maxInterval;

        public float Elapsed { get; protected set; }

        public void Reset()
        {
            Elapsed = 0.0f;
            counter = 0.0f;
            minInterval = TickInterval - RandomDeviation;
            maxInterval = TickInterval + RandomDeviation;
        }

        public bool Tick(float deltaTime)
        {
            Elapsed += deltaTime;
            counter += deltaTime;

            if ((RandomDeviation == 0.0f && counter >= TickInterval)
                || (counter >= minInterval && counter <= maxInterval))
            {
                counter = 0.0f;
                return true;
            }

            return false;
        }
    }

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
