using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskMoveTowards2D : BTBaseTask<BTTaskMoveTowards2DProp>
	{
		public override string Name => "Move Towards 2D";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskMoveTowards2DProp prop)
        {

        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskMoveTowards2DProp prop)
        {
			var actorPos = actor.transform.position;
			var targetPos = blackboard.GetValue<Vector2>(prop.TargetPosition);
			var distVect = new Vector2(targetPos.x - actorPos.x, targetPos.y - actorPos.y);

			if (Vector2.SqrMagnitude(distVect) > Vector2.kEpsilonNormalSqrt)
			{
				actor.transform.Translate(distVect.normalized * Time.deltaTime * prop.Speed);
                blackboard.Update<Vector2>(prop.Velocity, new Vector2(distVect.x, distVect.y));
				return BTNodeState.RUNNING;
			}

            blackboard.Update<Vector2>(prop.Velocity, Vector2.zero);
            return BTNodeState.SUCCESS;
        }
    }

	[System.Serializable]
	public class BTTaskMoveTowards2DProp
	{
		public float Speed;
		[BlackboardValue(typeof(Vector2))] public string TargetPosition;
		[BlackboardValue(typeof(Vector2))] public string Velocity;
	}
}