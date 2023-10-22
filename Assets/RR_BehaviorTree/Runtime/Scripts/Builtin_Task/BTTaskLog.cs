using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTTaskBase
    {
        [SerializeField]
        private string _message = string.Empty;

        public override string Name => "Log";

        protected override BTNodeState OnUpdate()
        {
            Debug.Log(_message);
            return BTNodeState.Success;
        }
    }
}
