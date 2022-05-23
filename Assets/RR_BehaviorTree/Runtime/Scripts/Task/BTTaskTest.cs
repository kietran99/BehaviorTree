using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskTest : BTBaseTask<BTTaskTestProp>
    {
        public override string Name => "Test";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskTestProp prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskTestProp prop)
        {
            return BTNodeState.Failure;
        }
    }

    [System.Serializable]
    public class BTTaskTestProp
    {
        public int MyInt;
    }
}
