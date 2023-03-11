namespace RR.AI.BehaviorTree
{
    public class BTTaskDummy : BTTaskBase
    {
        public override string Name => "Dummy";

        protected override BTNodeState OnUpdate() => BTNodeState.Success;
    }
}
