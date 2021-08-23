namespace RR.AI.BehaviorTree
{
	public class BTTaskMoveTowards : BTBaseTask<BTTaskMoveTowardsData>
	{
		public override string Name => "Move Towards";

        public override void Init(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            
        }

        public override BTNodeState Tick(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskMoveTowardsData
	{
		// public UnityEngine.Transform target;
		public float Speed;
		public UnityEngine.Vector3 TargetPosition;
	}
}