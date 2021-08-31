namespace RR.AI.BehaviorTree
{
    public interface IBTSavable
    {
        string Guid { get; }
        System.Action DeleteCallback { get; }
        void Save(BTDesignContainer designContainer);
    }
}
