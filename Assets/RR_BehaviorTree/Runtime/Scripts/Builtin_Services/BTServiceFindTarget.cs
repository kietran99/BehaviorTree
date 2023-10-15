using UnityEngine;

using RR.Serialization;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTServiceFindTarget: BTServiceBase
    {
        public override string Name => "Find Target";

        [SerializeField]
        private float _range = 3.0f;

        [SerializeField]
        [TagField]
        private string _targetTag;

        // [SerializeField]
        // [LayerMaskField]
        // private int _targetLayers;

        [SerializeField]
        private BBKeySelectorObject _targetKey;

        private GameObject _foundTarget = null;

        protected override void OnSearchStart()
        {
            _foundTarget = null;
        }

        protected override void OnEvaluate()
        {
            if (_foundTarget)
            {
                return;
            }

            Collider2D[] targets = Physics2D.OverlapCircleAll(_actor.transform.position, _range);
            if (targets.Length == 0)
            {
                return;
            }

            Collider2D firstValidTarget = targets.FirstOrDefault(target => target.gameObject != _actor && target.CompareTag(_targetTag));
            if (!firstValidTarget)
            {
                return;
            }

            _foundTarget = firstValidTarget.gameObject;
            _targetKey.UpdateValue(_blackboard, _foundTarget);

            // var filter = new ContactFilter2D();
            // filter.useLayerMask = true;
            // filter.SetLayerMask(_targetLayers);

            // int nTargetsInRangeWithoutTag = Physics2D.OverlapCircle(_actor.transform.position, _range, filter, _targets);
            
            // if (nTargetsInRangeWithoutTag == 0)
            // {
            //     return;
            // }

            // int nTargetsInRangeWithTag = 0;

            // for (int i = 0; i < _targets.Length; i++)
            // {
            //     nTargetsInRangeWithTag += _targets[i].CompareTag(_targetTag) ? 1 : 0;
            // }
            
            // if (nTargetsInRangeWithTag > 0)
            // {
            //     Debug.Log("Found Target");
            //     _target.UpdateValue(_blackboard, _targets[0].gameObject);
            // }
        }
    }
}
