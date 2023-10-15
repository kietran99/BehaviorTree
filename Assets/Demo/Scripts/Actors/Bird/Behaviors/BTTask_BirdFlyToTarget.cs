using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public delegate void BirdFlyTargetArriveEvent(Transform roostTarget);

    public class BTTask_BirdFlyToTarget : BTTaskBase
    {
        public override string Name => "Fly to Target";

        [SerializeField]
        private BBKeySelectorObject _targetKey = null;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float _speed = 1.0f;

        [SerializeField]
        private float _acceptableDist = 0.44f;

        [SerializeField]
        private BBKeySelectorBool _isGroundedKey = null;

        [SerializeField]
        private bool _flyToGround = false;

        private const string ANIM_IDLE_FRONT = "A_Bird_0_Idle_Front";
        private const string ANIM_IDLE_BACK = "A_Bird_0_Idle_Back";
        private const string ANIM_FLY_WINDUP_FRONT = "A_Bird_0_Fly_Windup_Front";
        private const string ANIM_FLY_WINDUP_BACK = "A_Bird_0_Fly_Windup_Back";

        private Transform _actorTransform;
        private Animator _actorAnimator;
        private SpriteRenderer _actorRenderer;

        private bool _shouldStartMoving;

        protected override void OnStart()
        {
            _actorTransform = _actor.transform;
            _actorAnimator = _actor.GetComponent<Animator>();
            _actorRenderer = _actor.GetComponent<SpriteRenderer>();
        }

        protected override void OnEnter()
        {
            _shouldStartMoving = false;

            {
                Vector2 actorPos = _actorTransform.position;
                ChangeAnim(TargetPos.y < actorPos.y ? ANIM_FLY_WINDUP_FRONT : ANIM_FLY_WINDUP_BACK, TargetPos.x > actorPos.x);
                _eventHub.Subscribe<BirdFlyAnimStartEvent>(OnFlyAnimStart);
            }
        }

        protected override BTNodeState OnUpdate()
        {
            if (!_shouldStartMoving)
            {
                return BTNodeState.Running;
            }

            float offsetY = _actorRenderer.flipX ? 0.05f : -0.2f;
            Vector2 targetPos = TargetPos + new Vector2(0.0f, offsetY);
            Vector2 moveVec = targetPos - (Vector2)_actor.transform.position;
            var (moveX, moveY) = (moveVec.x, moveVec.y);
            float realSpeed = Time.deltaTime * _speed;
            _actorTransform.Translate(new Vector3(moveX * realSpeed, moveY * realSpeed, 0f));            
            bool hasReachedTarget = moveVec.sqrMagnitude < _acceptableDist * _acceptableDist;
            if (hasReachedTarget)
            {
                SetActorShadowActive(true);
                _eventHub.Publisher<BirdFlyTargetArriveEvent>()?.Invoke((_blackboard[_targetKey] as GameObject).transform);
            }
            return hasReachedTarget ? BTNodeState.Success : BTNodeState.Running;
        }

        protected override void OnExit()
        {
            _eventHub.Unsubscribe<BirdFlyAnimStartEvent>(OnFlyAnimStart);
            ChangeAnim(TargetPos.y < _actorTransform.position.y ? ANIM_IDLE_FRONT : ANIM_IDLE_BACK, _actorRenderer.flipX);
            _isGroundedKey.UpdateValue(_blackboard, _flyToGround);
        }

        private Vector2 TargetPos => (_blackboard[_targetKey] as GameObject).transform.position;

        private void ChangeAnim(string animName, bool flipX)
        {
            _actorAnimator.Play(animName);
            _actorRenderer.flipX = flipX;
        }

        private void OnFlyAnimStart()
        {
            _shouldStartMoving = true;
            _isGroundedKey.UpdateValue(_blackboard, false);
            SetActorShadowActive(false);
        }

        private void SetActorShadowActive(bool value)
        {
            if (_actorTransform.GetChild(0).TryGetComponent<SpriteRenderer>(out var shadowRenderer))
            {
                shadowRenderer.enabled = value;
            }
        }
    }
}
