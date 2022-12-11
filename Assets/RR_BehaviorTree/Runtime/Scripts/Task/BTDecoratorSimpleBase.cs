namespace RR.AI.BehaviorTree
{
    public abstract class BTDecoratorSimpleBase: BTTaskBase
    {
        protected sealed override BTNodeState OnUpdate() => OnEvaluate().ToBTNodeState();

        protected abstract BTDecoState OnEvaluate();
    }
}
