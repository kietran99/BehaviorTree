namespace RR.AI.BehaviorTree
{
	public static class BTNullTickBehavior
	{
		public static BTNodeState Tick(UnityEngine.GameObject actor, Blackboard blackboard) 
		{
			return BTNodeState.FAILURE;
		}
	}
}