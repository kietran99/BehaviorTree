using UnityEngine;

namespace RR.AI.BehaviorTree
{
	public class BTTaskWithinRange : BTBaseTask<BTTaskWithinRangeData>
	{
		public override string Name => "Within Range";

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, BTTaskWithinRangeData prop)
        {
            prop.Targets = new Collider2D[1];
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, BTTaskWithinRangeData prop)
        {
            var filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(prop.TargetLayers);

            int nTargetsInRangeWithoutTag = Physics2D.OverlapCircle(actor.transform.position, prop.Range, filter, prop.Targets);
            
            if (nTargetsInRangeWithoutTag == 0)
            {
                return nTargetsInRangeWithoutTag.ToBTNodeState();
            }

            int nTargetsInRangeWithTag = 0;

            for (int i = 0; i < prop.Targets.Length; i++)
            {
                nTargetsInRangeWithTag += prop.Targets[i].CompareTag(prop.TargetTag) ? 1 : 0;
            }
            
            if (nTargetsInRangeWithTag > 0)
            {
                blackboard.Update<Vector2>(prop.TargetPosition, prop.Targets[0].transform.position);
            }

            return nTargetsInRangeWithTag.ToBTNodeState();
        }
    }

	[System.Serializable]
	public class BTTaskWithinRangeData
	{
		public float Range;
        
        [RR.Serialization.TagField] public string TargetTag;
        [RR.Serialization.LayerMaskField] public int TargetLayers;
        [BlackboardValue(typeof(Vector2))] public string TargetPosition;

        public Collider2D[] Targets { get; set; }
	}
}