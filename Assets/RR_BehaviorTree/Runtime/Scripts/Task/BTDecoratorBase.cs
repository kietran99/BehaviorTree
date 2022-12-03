namespace RR.AI.BehaviorTree
{
    public abstract class BTDecoratorBase: BTTaskBase
    {
        protected sealed override BTNodeState OnUpdate() => OnEvaluate().ToBTNodeState();

        protected abstract BTDecoState OnEvaluate();
    }
}
