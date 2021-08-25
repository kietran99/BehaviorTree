using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTBaseTask<BTTaskLogData>
    {
        public override string Name => "Log";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskLogData prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskLogData prop)
        {
            UnityEngine.Debug.Log(prop.Message);
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
