using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTBaseTask<BTTaskLogData>
    {
        public override string Name => "Log";

        public override void Init(GameObject actor, Blackboard bLackboard, string nodeGuid)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard bLackboard, string nodeGuid)
        {
            Debug.Log(Prop(nodeGuid).Message);
            return BTNodeState.SUCCESS;
        }
    }

    [System.Serializable]
    public class BTTaskLogData
    {
        public string Message;
        // public TestValue Container;
    }
}
