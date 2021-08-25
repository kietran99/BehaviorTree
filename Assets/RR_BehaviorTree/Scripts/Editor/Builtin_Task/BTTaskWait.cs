using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskWait : BTBaseTask<BTTaskWaitData>
    {
        public override string Name => "Wait";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskWaitData prop)
        {
            
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskWaitData prop)
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