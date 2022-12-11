namespace RR.AI.BehaviorTree
{
    public class BTDecoFailer : BTDecoratorSimpleBase
    {
        public override string Name => "Failer";

        protected override BTDecoState OnEvaluate() => BTDecoState.FAILURE;
    }
}
