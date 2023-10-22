namespace RR.AI.BehaviorTree
{
    public class BTDecoSucceeder : BTDecoratorSimpleBase
    {
        public override string Name => "Succeeder";

        protected override BTDecoState OnEvaluate() => BTDecoState.SUCCESS;
    }
}
