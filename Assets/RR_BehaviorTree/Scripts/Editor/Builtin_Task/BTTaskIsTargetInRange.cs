namespace RR.AI.BehaviorTree
{
	public class BTTaskIsTargetInRange : BTBaseTask<BTTaskIsTargetInRangeData>
	{
		public override string Name => "Is Target in Range";

        public override void Init(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            
        }

        public override BTNodeState Tick(UnityEngine.GameObject actor, Blackboard blackboard, string nodeGuid)
        {
            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskIsTargetInRangeData
	{
		public float Range;
        public string TargetTag;
	}
}