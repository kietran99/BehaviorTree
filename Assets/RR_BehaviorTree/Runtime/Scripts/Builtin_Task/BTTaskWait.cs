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
            _elapsed = 0f;
        }

        protected sealed override BTNodeState OnUpdate()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed < _duration)
            {
                return BTNodeState.Running;
            }

            _elapsed = 0.0f;
            return BTNodeState.Success;
        }
    }
}