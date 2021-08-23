namespace RR.AI.BehaviorTree
{
    public class BTTaskWait : BTBaseTask<BTTaskWaitData>
    {
        public override string Name => "Wait";

        public override void Init(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            
        }

        public override BTNodeState Tick(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskWaitData
	{
		public float Duration;
	}
}