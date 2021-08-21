using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public interface IBTNode
    {
        bool Init(IBTNode[] children, GameObject actor, Blackboard blackboard);
        BTNodeState Tick(GameObject actor, Blackboard blackboard);
    }
}
