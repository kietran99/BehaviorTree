namespace RR.AI.BehaviorTree
{
    public class BTSequencer : BTComposite
    {
        public BTSequencer(string guid) : base(guid)
        {}

        protected override BTNodeState FinalReturnState => BTNodeState.SUCCESS;

        // public override BTNodeState Update(UnityEngine.GameObject actor, Blackboard blackboard)
        // {
        //     OnTick?.Invoke(_guid);

        //     for (int i = 0; i < _children.Length; i++)
        //     {
        //         var res = _children[i].Update(actor, blackboard);

        //         if (res != BTNodeState.SUCCESS)
        //         {
        //             return res;
        //         }
        //     }

        //     return BTNodeState.SUCCESS;
        // }
    }
}