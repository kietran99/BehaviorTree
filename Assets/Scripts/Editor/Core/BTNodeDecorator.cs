using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTNodeDecorator : BTNode
    {
        public BTNodeDecorator(Vector2 pos, BTNodeType nodeType, string name="Decorator", string guid="") 
            : base(pos, nodeType, name, guid)
        {
            CreatePorts(PortCapacity.Single, PortCapacity.Single);
        }
    }
}
