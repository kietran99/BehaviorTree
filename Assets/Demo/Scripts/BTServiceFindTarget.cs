using UnityEngine;

using RR.AI.BehaviorTree;
using RR.AI;
using RR.Serialization;

namespace RR.Demo.AI.BehaviorTree
{
    public class BTServiceFindTarget: BTServiceBase
    {
        [SerializeField]
        private float _range;

        [SerializeField]
        [TagField]
        private string _targetTag;

        [SerializeField]
        [LayerMaskField]
        private int _targetLayers;

        [SerializeField]
        [BlackboardValue(typeof(Object))]
        private string _target;

        public override string Name => "Find Target";

        protected override void OnEvaluate()
        {
            Debug.Log("Find Target");
        }
    }
}
