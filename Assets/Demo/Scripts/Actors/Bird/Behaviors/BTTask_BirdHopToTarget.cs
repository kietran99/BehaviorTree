using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_BirdHopToTarget : BTTaskBase
    {
        public override string Name => "Hop to Target";

        [SerializeField]
        private BBKeySelectorVector2 _targetKey = null;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        private float _moveSpeed = 1.0f;

        [SerializeField]
        private float _acceptableDist = 0.55f;

        private const string ANIM_IDLE_FRONT = "A_Bird_0_Idle_Front";
        private const string ANIM_IDLE_BACK = "A_Bird_0_Idle_Back";
        private const string ANIM_MOVE_FRONT = "A_Bird_0_Move_Front";
        private const string ANIM_MOVE_BACK = "A_Bird_0_Move_Back";

        private Animator _animator;
        private SpriteRenderer _renderer;
        private float _lastMoveX, _lastMoveY;

        protected override void OnStart()
        {
            _animator = _actor.GetComponent<Animator>();
            _renderer = _actor.GetComponent<SpriteRenderer>();
            _lastMoveX = _lastMoveY = 0.0f;
        }

        protected override BTNodeState OnUpdate()
        {
            float realSpeed = Time.deltaTime * _moveSpeed;
            Vector2 targetPos = _targetKey.Value(_blackboard);
            Vector2 moveVec = targetPos - (Vector2)_actor.transform.position;
            var normalizedMoveVec = moveVec.normalized;
            var (moveX, moveY) = (normalizedMoveVec.x, normalizedMoveVec.y);
            _actor.transform.Translate(new Vector3(moveX * realSpeed, moveY * realSpeed, 0f));
            bool hasReachedTarget = moveVec.sqrMagnitude < _acceptableDist;
            ChangeAnimState(hasReachedTarget ? Vector2.zero : new Vector2(moveX, moveY));
            _lastMoveX = moveX != 0.0f ? moveX : _lastMoveX;
            _lastMoveY = moveY != 0.0f ? moveY : _lastMoveY;
            return hasReachedTarget ? BTNodeState.Success : BTNodeState.Running;
        }

        private void ChangeAnimState(Vector2 moveVec)
        {
            (float moveX, float moveY) = (moveVec.x, moveVec.y);
            if (moveX == 0.0f && moveY == 0.0f)
            {
                _animator.Play(_lastMoveY < 0.0f ? ANIM_IDLE_FRONT : ANIM_IDLE_BACK);
                _renderer.flipX = _lastMoveX > 0.0f;
                return;
            }

            _animator.Play(moveY <= 0.0f ? ANIM_MOVE_FRONT : ANIM_MOVE_BACK);
            _renderer.flipX = moveX > 0.0f;
        }
    }
}
