using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTBaseTask<BTTaskLogData>
    {
        public override string Name => "Log";

        public override void Init(GameObject actor, Blackboard bLackboard)
        {
            Debug.Log("Init: Log");
        }

        public override BTNodeState Tick(GameObject actor, Blackboard bLackboard)
        {
            Debug.Log("Tick: Log");
            return BTNodeState.SUCCESS;
        }
    }

    [System.Serializable]
    public class BTTaskLogData
    {
        public string Message;
        public int myInt;
        public bool myBool;
        // public TestValue Container;
    }
}
