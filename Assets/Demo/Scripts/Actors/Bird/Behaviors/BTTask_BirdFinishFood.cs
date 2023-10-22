using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_BirdFinishFood : BTTaskBase
    {
        public override string Name => "Finish Food";

        [SerializeField]
        private BBKeySelectorObject _targetKey = null;

        protected override BTNodeState OnUpdate()
        {
            Destroy(_blackboard[_targetKey]);
            return BTNodeState.Success;
        }
    }
}
