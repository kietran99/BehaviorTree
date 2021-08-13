using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTNodeComposite : BTNode
    {
        public BTNodeComposite(Vector2 pos, BTNodeType taskType, string name="Composite", string guid="") 
            : base(pos, taskType, name, guid)
        {
            CreatePorts(PortCapacity.Single, PortCapacity.Multi);
        }
    }
}
