using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTServiceBase : BTTaskBase
    {
        [SerializeField]
        private float TickInterval = 1.0f;
        [SerializeField]
        private float RandomDeviation = 0.2f;

        private float _counter;
        private float _minInterval, _maxInterval;
        
        public float Elapsed { get; protected set; }

        public void Reset()
        {
            Elapsed = 0.0f;
            _counter = 0.0f;
            _minInterval = TickInterval - RandomDeviation;
            _maxInterval = TickInterval + RandomDeviation;
        }

        public bool Tick(float deltaTime)
        {
            _counter += deltaTime;
            Elapsed += deltaTime;

            if (RandomDeviation == 0.0f)
            {
                if (_counter >= TickInterval)
                {
                    _counter = 0.0f;
                    return true;
                }

                return false;
            }

            float curInterval = UnityEngine.Random.Range(0, 2) == 0 ? _minInterval : _maxInterval;
            if (_counter >= curInterval)
            {
                _counter = 0.0f;
                return true;
            }

            return false;
        }

        protected sealed override void OnStart()
        {
            Reset();
            OnSearchStart();
        }

        protected sealed override BTNodeState OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            bool finishedInterval = Tick(deltaTime);
            if (finishedInterval)
            {
                OnEvaluate();
            }
            
            return BTNodeState.Success; // Return anything is ok as it won't be evaluated
        }

        protected override void OnTreeEval()
        {
            Reset();
        }

        protected virtual void OnSearchStart() {}
        protected abstract void OnEvaluate();
    }
}
