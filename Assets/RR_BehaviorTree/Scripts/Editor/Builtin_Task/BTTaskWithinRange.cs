using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskWithinRange : BTBaseTask<BTTaskWithinRangeData>
	{
		public override string Name => "Within Range";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskWithinRangeData prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskWithinRangeData prop)
        {
            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskWithinRangeData
	{
		public float Range;
        public string TargetTag;
	}
}