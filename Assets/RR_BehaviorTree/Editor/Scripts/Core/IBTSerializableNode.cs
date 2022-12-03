using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public interface IBTSerializableNode : IBTIdentifiable, IBTGraphOrderable
    {
        void OnCreate(BTGraphDesign designContainer, Vector2 position);
        void OnMove(BTGraphDesign designContainer, Vector2 moveDelta);
        void OnConnect(BTGraphDesign designContainer, string parentGuid);
        void OnDelete(BTGraphDesign designContainer);
    }
}
