using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskTriggerAnimation : BTBaseTask<BTTaskTriggerAnimationData>
    {
        public override string Name => "Trigger Animation";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskTriggerAnimationData prop)
        {
            if (!actor.TryGetComponent<Animator>(out var animator))
            {
                return;
            }

            prop.ActorAnimator = animator;
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskTriggerAnimationData prop)
        {
            prop.ActorAnimator.SetTrigger(prop.Name);
            return BTNodeState.Success;
        }
    }

    [System.Serializable]
    public class BTTaskTriggerAnimationData
    {
        public string Name;

        public Animator ActorAnimator { get; set; }
    }
}
