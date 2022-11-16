using UnityEngine;

using RR.AI.BehaviorTree;
using RR.AI;

namespace RR.Demo.AI.BehaviorTree
{
    public class BTServiceNone : BTServiceBase<BTPropServiceNone>
    {
        public override string Name => "Service None";

        protected override void OnStart(GameObject actor, RuntimeBlackboard blackboard, BTPropServiceNone prop)
        {
            
        }

        protected override void OnUpdate(GameObject actor, RuntimeBlackboard blackboard, BTPropServiceNone prop)
        {
            Debug.Log(prop.Message);
        }
    }

    [System.Serializable]
    public class BTPropServiceNone : BTPropServiceBase
    {
        public string Message;
    }
}
