using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskTest : BTBaseTask<BTTaskTestProp>
    {
        public override string Name => "Test";

        public override void Init(GameObject actor, Blackboard bLackboard, string nodeGuid)
        {
        
        }

        public override BTNodeState Tick(GameObject actor, Blackboard bLackboard, string nodeGuid)
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
