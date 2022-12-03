namespace RR.AI.BehaviorTree
{
    public class BTDecoSucceeder : BTDecoratorBase
    {
        public override string Name => "Succeeder";

        protected override BTDecoState OnEvaluate() => BTDecoState.SUCCESS;
    }
}
