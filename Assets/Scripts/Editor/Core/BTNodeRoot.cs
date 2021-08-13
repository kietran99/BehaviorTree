using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTNodeRoot : BTNode
    {
        public BTNodeRoot(Vector2 pos, BTNodeType taskType, string name="Root", string guid="") : base(pos, taskType, name, guid)
        {
            CreatePorts(PortCapacity.None, PortCapacity.Single);
            capabilities &= ~UnityEditor.Experimental.GraphView.Capabilities.Movable;
            capabilities &= ~UnityEditor.Experimental.GraphView.Capabilities.Deletable;
        }
    }
}
