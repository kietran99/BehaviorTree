namespace RR.AI.BehaviorTree
{
    public interface IBTLifeCycleReceiver
    {
        string Name { get; }

        void Init(BTRuntimeContext context);
        BTNodeState Update();
        void TreeEval();
        void Abort();
    }
}
