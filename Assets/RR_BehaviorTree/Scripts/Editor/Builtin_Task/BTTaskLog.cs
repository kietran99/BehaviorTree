namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTBaseTask<BTTaskLogData>
    {
        public override string Name => "Log";

        public override void Init(UnityEngine.GameObject actor, Blackboard bLackboard, string nodeGuid)
        {
            
        }

        public override BTNodeState Tick(UnityEngine.GameObject actor, Blackboard bLackboard, string nodeGuid)
        {
            UnityEngine.Debug.Log(Prop(nodeGuid).Message);
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
