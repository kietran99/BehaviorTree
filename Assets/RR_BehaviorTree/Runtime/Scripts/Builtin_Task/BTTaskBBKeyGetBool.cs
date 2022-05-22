using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskBBKeyGetBool : BTBaseTask<BTTaskBBKeyGetBoolProp>
    {
        public override string Name => "BB Key Get Bool";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskBBKeyGetBoolProp prop)
        {
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskBBKeyGetBoolProp prop)
        {
            return (blackboard.GetValue<bool>(prop.Key) == prop.IsSet).ToBTNodeState();
        }
    }

    [System.Serializable]
	public class BTTaskBBKeyGetBoolProp
	{
		[BlackboardValue(typeof(bool))]
		public string Key;
		public bool IsSet;
	}
}