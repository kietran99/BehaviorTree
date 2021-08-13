using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTSelector : IBTNodeAction
    {
        public string Name => "Selector";

        public BTNodeType NodeType => BTNodeType.Selector;
        
        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.Multi);

        public void Tick(GameObject actor, UnityEditor.Experimental.GraphView.Blackboard bLackboard)
        {
            
        }
    }
}
