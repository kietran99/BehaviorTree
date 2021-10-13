using UnityEngine;
using RR.AI.BehaviorTree;
using RR.AI;

namespace RR.Demo.AI.BehaviorTree
{
    public class IsSkeletonActive : BTBaseTask<IsSkeletonActiveProp>
    {
        public override string Name => "Is Skeleton Active";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, IsSkeletonActiveProp prop)
        {
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, IsSkeletonActiveProp prop)
        {
            var isSkeletonActive = !blackboard.ValueOr(prop.ActivatedKey, false);
            return isSkeletonActive.ToBTNodeState();
        }
    }

    [System.Serializable]
    public class IsSkeletonActiveProp
    {
        [BlackboardValue(typeof(bool))]
        public string ActivatedKey;
    }
}
