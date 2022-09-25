using UnityEngine;

using RR.AI.BehaviorTree;
using RR.AI;
using RR.Serialization;

namespace RR.Demo.AI.BehaviorTree
{
    public class BTServiceFindTarget : BTServiceBase<BTPropServiceFindTarget>
    {
        public override string Name => "Find Target";

        protected override void OnStart(GameObject actor, RuntimeBlackboard blackboard, BTPropServiceFindTarget prop)
        {
            
        }

        protected override void OnUpdate(GameObject actor, RuntimeBlackboard blackboard, BTPropServiceFindTarget prop)
        {
            
        }
    }

    [System.Serializable]
    public class BTPropServiceFindTarget : BTPropServiceBase
    {
        public float Range;
        [TagField] public string TargetTag;
        [LayerMaskField] public int TargetLayers;
        [BlackboardValue(typeof(Object))] public string Target;
    }
}
