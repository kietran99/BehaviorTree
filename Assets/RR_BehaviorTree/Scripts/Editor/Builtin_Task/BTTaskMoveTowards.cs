using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskMoveTowards : BTBaseTask<BTTaskMoveTowardsData>
	{
		public override string Name => "Move Towards";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskMoveTowardsData prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskMoveTowardsData prop)
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