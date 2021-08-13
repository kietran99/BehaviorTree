using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTSequencer : IBTNodeAction
    {
        public string Name => "Sequencer";

        public BTNodeType NodeType => BTNodeType.Sequencer;
        
        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.Multi);

        public void Tick(GameObject actor, UnityEditor.Experimental.GraphView.Blackboard bLackboard)
        {
            
        }
    }
}
