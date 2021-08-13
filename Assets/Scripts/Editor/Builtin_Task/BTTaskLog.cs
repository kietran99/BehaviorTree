using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskLog : BTBaseTask
    {
        public override string Name => "Log";

        public BTNodeType NodeType => BTNodeType.Leaf;

        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.None);

        public override void Init(GameObject actor, Blackboard bLackboard)
        {
            
        }

        public override void Tick(GameObject actor, Blackboard bLackboard)
        {
            
        }
    }
}
