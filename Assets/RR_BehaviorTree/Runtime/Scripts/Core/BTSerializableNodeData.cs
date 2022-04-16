using UnityEngine;
using System;

namespace RR.AI.BehaviorTree
{
    [Serializable]
    public class BTSerializableNodeData : BTSerializableNodeDataBase
    {
        [SerializeField]
        private BTNodeType _nodeType = BTNodeType.Root;

        public BTSerializableNodeData(
            Vector2 position, string name, string description, string guid, string parentGuid, BTNodeType nodeType)
            : base(position, name, description, guid, parentGuid)
        {
            _nodeType = nodeType;
        }

        public BTNodeType NodeType => _nodeType;
    }
}