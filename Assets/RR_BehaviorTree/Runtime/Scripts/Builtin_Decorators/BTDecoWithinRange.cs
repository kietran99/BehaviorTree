using UnityEngine;

using RR.Serialization;

namespace RR.AI.BehaviorTree
{
    public class BTDecoWithinRange : BTDecoratorBase
	{
		public override string Name => "Within Range";

        public float Range;
        [TagField] public string TargetTag;
        [LayerMaskField] public int TargetLayers;
        [BlackboardValue(typeof(Vector3))] public string TargetPosition;

        public Collider2D[] Targets { get; set; }

        protected override void OnStart()
        {
            Targets = new Collider2D[1];
        }

        protected override BTDecoState OnEvaluate()
        {
            var filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(TargetLayers);

            int nTargetsInRangeWithoutTag = Physics2D.OverlapCircle(_actor.transform.position, Range, filter, Targets);
            
            if (nTargetsInRangeWithoutTag == 0)
            {
                return nTargetsInRangeWithoutTag.ToBTDecoState();
            }

            int nTargetsInRangeWithTag = 0;

            for (int i = 0; i < Targets.Length; i++)
            {
                nTargetsInRangeWithTag += Targets[i].CompareTag(TargetTag) ? 1 : 0;
            }
            
            if (nTargetsInRangeWithTag > 0)
            {
                Vector2 newTargetPos = Targets[0].transform.position;
                _blackboard.UpdateEntry<Vector2>(TargetPosition, newTargetPos);
            }

            return nTargetsInRangeWithTag.ToBTDecoState();
        }
    }
}
