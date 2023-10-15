using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_PlayAnimation : BTTaskBase
    {
        public override string Name => "Play Animation";

        [SerializeField]
        private AnimationStatePicker _animationStatePicker;

        [SerializeField]
        private bool _nonBlocking = false;

        [SerializeField]
        private float _startTime = 0.0f;

        private Animator _animator;

        protected override void OnStart()
        {
            _animator = _actor.GetComponent<Animator>();
        }

        protected override void OnEnter()
        {
            _animator.Play(_animationStatePicker.StateName, _animationStatePicker.Layer, _startTime);
        }

        protected override BTNodeState OnUpdate()
        {
            if (_nonBlocking)
            {
                return BTNodeState.Success;
            }

            AnimatorStateInfo animStateInfo = _animator.GetCurrentAnimatorStateInfo(_animationStatePicker.Layer);
            return IsAnimPlaying(animStateInfo, _animationStatePicker.StateName) ? BTNodeState.Success : BTNodeState.Running;
        }

        private bool IsAnimPlaying(AnimatorStateInfo animStateInfo, string name)
        {
            return animStateInfo.IsName(name) && animStateInfo.normalizedTime >= 1.0f;
        }
    }
}
