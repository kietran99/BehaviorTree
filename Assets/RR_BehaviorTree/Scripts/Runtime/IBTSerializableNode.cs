namespace RR.AI.BehaviorTree
{
    public interface IBTSerializableNode
    {
        string Guid { get; }
        void OnCreate(BTDesignContainer designContainer, UnityEngine.Vector2 position);
        void OnMove(BTDesignContainer designContainer, UnityEngine.Vector2 position);
        void OnConnect(BTDesignContainer designContainer, string parentGuid);
        void OnDelete(BTDesignContainer designContainer);
    }
}
