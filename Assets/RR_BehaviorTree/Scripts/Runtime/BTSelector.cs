namespace RR.AI.BehaviorTree
{
    public class BTSelector : BTComposite
    {
        public BTSelector(string guid) : base(guid)
        {}

        public override BTNodeState Tick(UnityEngine.GameObject actor, Blackboard blackboard)
        {
            for (int i = 0; i < _children.Length; i++)
            {
                var res = _children[i].Tick(actor, blackboard);
                
                if (res != BTNodeState.FAILURE)
                {
                    return res;
                }
            }

            return BTNodeState.FAILURE;
        }
    }
}