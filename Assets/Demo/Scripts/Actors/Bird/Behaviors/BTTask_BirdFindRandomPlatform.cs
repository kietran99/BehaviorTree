using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_BirdFindRandomPlatform : BTTaskBase
    {
        public override string Name => "Find Random Platform";

        [SerializeField]
        [Serialization.TagField]
        private string _tag;

        [SerializeField]
        private BBKeySelectorObject _targetKey = null;

        private GameObject[] _platforms;

        protected override void OnStart()
        {
            _platforms = GameObject.FindGameObjectsWithTag(_tag);
        }

        protected override BTNodeState OnUpdate()
        {
            if (_platforms.Length == 0)
            {
                return BTNodeState.Failure;
            }

            GameObject target = _platforms[Random.Range(0, _platforms.Length)].transform.GetChild(0).gameObject;
            bool updateSuccess = _targetKey.UpdateValue(_blackboard, target);
            return updateSuccess.ToBTNodeState();
        }
    }
}
