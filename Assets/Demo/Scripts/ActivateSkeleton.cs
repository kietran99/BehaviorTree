using RR.AI;
using RR.AI.BehaviorTree;
using UnityEngine;

namespace RR.Demo.AI.BehaviorTree
{
    public class ActivateSkeleton : BTBaseTask<ActivateSkeletonProp>
    {
        public override string Name => "Activate";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, ActivateSkeletonProp prop)
        {
            if (!actor.TryGetComponent<Animator>(out var animator))
            {
                Debug.LogError($"No Animator was attached to {actor.name}");
                return;
            }

            prop.ActorAnimator = animator;
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, ActivateSkeletonProp prop)
        {
            prop.ActorAnimator.SetTrigger(prop.AnimParam);
            return blackboard.Update<bool>(prop.ActivatedKey, true).ToBTNodeState();
        }
    }

    [System.Serializable]
    public class ActivateSkeletonProp
    {
        public string AnimParam;
        [BlackboardValue(typeof(bool))]
        public string ActivatedKey;

        public Animator ActorAnimator { get; set; }
    }
}
