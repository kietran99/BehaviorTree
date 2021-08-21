using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTSequencer : BTComposite
    {
        public override BTNodeState Tick(GameObject actor, Blackboard blackboard)
        {
            for (int i = 0; i < _children.Length; i++)
            {
                var res = _children[i].Tick(actor, blackboard);
                
                if (res != BTNodeState.SUCCESS)
                {
                    return res;
                }
            }

            return BTNodeState.SUCCESS;
        }
    }
}