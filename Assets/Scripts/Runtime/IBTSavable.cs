namespace RR.AI.BehaviorTree
{
    public interface IBTSavable
    {
        string Guid { get; }
        void Save(BTDesignContainer designContainer);
    }
}
