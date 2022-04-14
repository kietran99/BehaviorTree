namespace RR.AI.BehaviorTree
{
    public class BTSelector : BTComposite
    {
        public BTSelector(string guid) : base(guid)
        {}

        protected override BTNodeState FinalReturnState => BTNodeState.FAILURE;

        // public override BTNodeState Update(UnityEngine.GameObject actor, Blackboard blackboard)
        // {
        //     OnTick?.Invoke(_guid);

        //     for (int i = 0; i < _children.Length; i++)
        //     {
        //         var res = _children[i].Update(actor, blackboard);

        //         if (res != BTNodeState.FAILURE)
        //         {
        //             // if (res == BTNodeState.RUNNING)
        //             // {
        //             //     BTBaseNode
        //             // }

        //             return res;
        //         }
        //     }

        //     return BTNodeState.FAILURE;
        // }
    }
}