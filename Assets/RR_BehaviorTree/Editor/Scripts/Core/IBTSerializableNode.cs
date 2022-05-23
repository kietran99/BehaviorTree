using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public interface IBTSerializableNode : IBTIdentifiable, IBTGraphOrderable
    {
        void OnCreate(BTDesignContainer designContainer, Vector2 position);
        void OnMove(BTDesignContainer designContainer, Vector2 moveDelta);
        void OnConnect(BTDesignContainer designContainer, string parentGuid);
        void OnDelete(BTDesignContainer designContainer);
    }
}
