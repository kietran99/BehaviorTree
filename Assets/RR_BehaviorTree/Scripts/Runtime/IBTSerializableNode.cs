namespace RR.AI.BehaviorTree
{
    public interface IBTSerializableNode
    {
        string Guid { get; }
        void OnCreate(BTDesignContainer designContainer, UnityEngine.Vector2 position);
        void OnDelete(BTDesignContainer designContainer);
    }
}
