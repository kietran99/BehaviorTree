using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public delegate void BirdFlyAnimStartEvent();

    public class BirdFlyBehavior : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!animator.gameObject.TryGetComponent(out BehaviorTree behaviorTree))
            {
                return;
            }
            
            behaviorTree.RuntimeEventHub.Publisher<BirdFlyAnimStartEvent>()?.Invoke();
        }
    }
}
