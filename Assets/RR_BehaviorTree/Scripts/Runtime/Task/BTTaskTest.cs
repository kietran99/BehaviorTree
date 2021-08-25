using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskTest : BTBaseTask<BTTaskTestProp>
    {
        public override string Name => "Test";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskTestProp prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskTestProp prop)
        {
            return BTNodeState.FAILURE;
        }
    }

    [System.Serializable]
    public class BTTaskTestProp
    {
        public int MyInt;
    }
}
