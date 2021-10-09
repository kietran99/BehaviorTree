using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskWithinRange : BTBaseTask<BTTaskWithinRangeData>
	{
		public override string Name => "Within Range";

        public override void Init(GameObject actor, Blackboard blackboard, BTTaskWithinRangeData prop)
        {
            prop.Targets = new Collider2D[prop.MaxTargets];
        }

        public override BTNodeState Tick(GameObject actor, Blackboard blackboard, BTTaskWithinRangeData prop)
        {
            var filter = new ContactFilter2D();
            int nTargetsInRangeWithoutTag = Physics2D.OverlapCircle(actor.transform.position, prop.Range, filter.NoFilter(), prop.Targets);
            
            if (nTargetsInRangeWithoutTag == 0)
            {
                return nTargetsInRangeWithoutTag.ToBTNodeState();
            }

            int nTargetsInRangeWithTag = 0;

            for (int i = 0; i < prop.Targets.Length; i++)
            {
                nTargetsInRangeWithTag += prop.Targets[i].CompareTag(prop.TargetTag) ? 1 : 0;
            }
            
            return nTargetsInRangeWithTag.ToBTNodeState();
        }
    }

	[System.Serializable]
	public class BTTaskWithinRangeData
	{
		public float Range;
        
        [RR.Serialization.TagField]
        public string TargetTag;
        public int MaxTargets;

        public Collider2D[] Targets { get; set; }
	}
}