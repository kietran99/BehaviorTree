using UnityEngine;

using RR.AI.BehaviorTree;

namespace RR.Demo.AI.BehaviorTree
{
    public class BTServiceNone : BTServiceBase
    {
        [SerializeField]
        private string _message = "None";

        public override string Name => "Service None";

        protected override void OnSearchStart()
        {
            
        }

        protected override void OnEvaluate()
        {
            Debug.Log(_message);
        }
    }
}
