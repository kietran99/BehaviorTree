using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskMoveTowards : BTBaseTask<BTTaskMoveTowardsProp>
	{
		public override string Name => "Move Towards";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskMoveTowardsProp prop)
        {

        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskMoveTowardsProp prop)
        {
			var actorPos = actor.transform.position;
			var targetPos = blackboard.GetValue<Vector3>(prop.TargetPosition);
			var distVect = new Vector3(targetPos.x - actorPos.x, targetPos.y - actorPos.y, targetPos.z - actorPos.z);

			if (Vector3.SqrMagnitude(distVect) > Vector3.kEpsilonNormalSqrt)
			{
				actor.transform.Translate(distVect.normalized * Time.deltaTime * prop.Speed);
				return BTNodeState.RUNNING;
			}

            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskMoveTowardsProp
	{
		public float Speed;
		[BlackboardValue(typeof(Vector3))]
		public string TargetPosition;
	}
}