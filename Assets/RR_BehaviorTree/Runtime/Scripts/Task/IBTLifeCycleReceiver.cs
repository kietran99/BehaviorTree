namespace RR.AI.BehaviorTree
{
    public interface IBTLifeCycleReceiver
    {
        string Name { get; }

        void Init(UnityEngine.GameObject actor, RuntimeBlackboard blackboard);
        BTNodeState Update();
        void TreeEval();
        void Abort();
    }
}
