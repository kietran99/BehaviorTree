using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_RandomLocation : BTTaskBase
    {
        public override string Name => "Pick Random Location";

        [SerializeField]
        private BBKeySelectorVector2 _targetKey = null;

        [SerializeField]
        private float _radius = 3.0f;

        private Transform _actorTransform;
        private Vector2 _screenBounds;

        protected override void OnStart()
        {
            _actorTransform = _actor.transform;
            _screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)) - _actor.GetComponent<SpriteRenderer>().bounds.size * 0.5f;
        }

        protected override BTNodeState OnUpdate()
        {
            float randomSqrMagnitude = Mathf.Pow(Random.Range(1.0f, _radius), 2.0f);
            float randomSqrX = Random.Range(1.0f, randomSqrMagnitude);
            float randomX = Mathf.Sqrt(randomSqrX) * RandomDirection;
            float randomY = Mathf.Sqrt(randomSqrMagnitude - randomSqrX) * RandomDirection;
            float clampedX = Mathf.Clamp(randomX + _actorTransform.position.x, -_screenBounds.x, _screenBounds.x);
            float clampedY = Mathf.Clamp(randomY + _actorTransform.position.y, -_screenBounds.y, _screenBounds.y);
            var randomLocation = new Vector2(clampedX, clampedY);
            bool updateSuccess = _targetKey.UpdateValue(_blackboard, randomLocation);
            return updateSuccess.ToBTNodeState();
        }

        private float RandomDirection => Random.Range(0, 2) == 0 ? 1 : -1;
    }
}
