using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRoot : IBTNodeAction
    {
        public string Name => "Root";

        public BTNodeType NodeType => BTNodeType.Root;

        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.None, BTPortCapacity.Single);

        public void Tick(GameObject actor, UnityEditor.Experimental.GraphView.Blackboard bLackboard)
        {
            
        }
    }
}
