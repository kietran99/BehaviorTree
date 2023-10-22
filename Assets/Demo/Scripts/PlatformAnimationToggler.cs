using UnityEngine;

namespace RR.AI.BehaviorTree
{
    [RequireComponent(typeof(Animator))]
    public class PlatformAnimationToggler : MonoBehaviour
    {
        [SerializeField]
        private BehaviorTree _actorBehaviorTree = null;

        private Animator _animator;

        private void Start()
        {
            _actorBehaviorTree.RuntimeEventHub.Subscribe<BirdFlyTargetArriveEvent>(OnBirdRoostEnter);
            _actorBehaviorTree.RuntimeEventHub.Subscribe<BirdFlyTargetExitEvent>(OnBirdRoostExit);
            _animator = GetComponent<Animator>();
        }

        private void OnBirdRoostEnter(Transform roostTarget)
        {
            if (transform.GetChild(0) != roostTarget)
            {
                return;
            }

            _animator.enabled = false;
        }

        private void OnBirdRoostExit()
        {
            if (!_animator.enabled)
            {
                _animator.enabled = true;
            }
        }
    }
}
