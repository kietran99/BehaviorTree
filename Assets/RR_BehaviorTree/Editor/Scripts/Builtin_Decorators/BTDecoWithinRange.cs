using UnityEngine;

using RR.Serialization;

namespace RR.AI.BehaviorTree
{
    public class BTDecoWithinRange : BTBaseDecorator<BTDecoWithinRangeData>
	{
		public override string Name => "Within Range";

        protected override void OnStart(GameObject actor, RuntimeBlackboard blackboard, BTDecoWithinRangeData prop)
        {
            prop.Targets = new Collider2D[1];
        }

        protected override BTDecoState OnUpdate(GameObject actor, RuntimeBlackboard blackboard, BTDecoWithinRangeData prop)
        {
            var filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(prop.TargetLayers);

            int nTargetsInRangeWithoutTag = Physics2D.OverlapCircle(actor.transform.position, prop.Range, filter, prop.Targets);
            
            if (nTargetsInRangeWithoutTag == 0)
            {
                return nTargetsInRangeWithoutTag.ToBTDecoState();
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

            return nTargetsInRangeWithTag.ToBTDecoState();
        }
    }

	[System.Serializable]
	public class BTDecoWithinRangeData
	{
		public float Range;
        
        [TagField] public string TargetTag;
        [LayerMaskField] public int TargetLayers;
        [BlackboardValue(typeof(Vector3))] public string TargetPosition;

        public Collider2D[] Targets { get; set; }
	}
}
