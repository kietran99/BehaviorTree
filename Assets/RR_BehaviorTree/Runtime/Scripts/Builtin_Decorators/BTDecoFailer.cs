namespace RR.AI.BehaviorTree
{
    public class BTDecoFailer : BTDecoratorBase
    {
        public override string Name => "Failer";

        protected override BTDecoState OnEvaluate() => BTDecoState.FAILURE;
    }
}
