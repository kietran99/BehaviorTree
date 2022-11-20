namespace RR.AI.BehaviorTree
{
    public abstract class BTPropServiceBase
    {
        public float TickInterval;
        public float RandomDeviation = 0.0f;

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
    }
}
