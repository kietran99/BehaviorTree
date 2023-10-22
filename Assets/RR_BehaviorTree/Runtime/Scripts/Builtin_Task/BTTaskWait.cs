using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskWait : BTTaskBase
    {
        [SerializeField]
        public float _duration = 0.0f;

        private float _elapsed;

        public override string Name => "Wait";

        protected override void OnStart()
        {
            ResetTimer();
        }

        protected sealed override BTNodeState OnUpdate()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed < _duration)
            {
                return BTNodeState.Running;
            }

            ResetTimer();
            return BTNodeState.Success;
        }

        protected sealed override void OnAbort()
        {
            ResetTimer();
        }

        private void ResetTimer() => _elapsed = 0.0f;
    }
}