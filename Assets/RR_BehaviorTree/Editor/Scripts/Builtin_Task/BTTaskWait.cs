using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskWait : BTBaseTask<BTTaskWaitData>
    {
        public override string Name => "Wait";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskWaitData prop)
        {
            prop.Elapsed = 0f;
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskWaitData prop)
        {   
            prop.Elapsed += Time.deltaTime;

            if (prop.Elapsed < prop.Duration)
            {
                return BTNodeState.Running;
            }

            prop.Elapsed = 0f;
            return BTNodeState.Success;
        }
    }

	[System.Serializable]
	public class BTTaskWaitData
	{
		public float Duration;

        public float Elapsed { get; set; }
	}
}