using UnityEngine;
using System;

namespace RR.AI.BehaviorTree
{
    [Serializable]
    public class BTNodeGraphData
    {
        [SerializeField]
        private string _guid = string.Empty;
        
        [SerializeField]
        private string _parentGuid = string.Empty;

        [SerializeField]
        private string _name = string.Empty;

        [SerializeField]
        private Vector2 _position = Vector2.zero;
        
        [SerializeField]
        [TextArea]
        private string _description = string.Empty;

        public BTNodeGraphData(Vector2 position, string name, string description, string guid, string parentGuid)
        {
            _guid = guid;
            _parentGuid = parentGuid;
            _position = position;
            _name = name;
            _description = description;
        }

        public string Guid => _guid;
        public string ParentGuid => _parentGuid;
        public string Name => _name;
        public Vector2 Position 
        {
            get => _position;
            set => _position = value;
        }

        public string Description => _description;    
    }

    [Serializable]
    public abstract class BTSerializableNodeDataBase
    {
        [SerializeField]
        private BTNodeGraphData _graphData = null;

        public BTSerializableNodeDataBase(Vector2 position, string name, string description, string guid, string parentGuid)
        {
            _graphData = new BTNodeGraphData(position, name, description, guid, parentGuid);
        }

        public string Guid => _graphData.Guid;
        public string ParentGuid => _graphData.ParentGuid;
        public string Name => _graphData.Name;
        public Vector2 Position 
        {
            get => _graphData.Position;
            set => _graphData.Position = value;
        }

        public string Description => _graphData.Description;
    }

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

    [Serializable]
    public class BTSerializableTaskData : BTSerializableNodeDataBase
    {
        [SerializeField]
        private BTBaseTask _task = null;

        public BTSerializableTaskData(
            Vector2 position, string name, string description, string guid, string parentGuid, BTBaseTask task)
            : base(position, name, description, guid, parentGuid)
        {
            _task = task;
        }

        public BTBaseTask Task => _task;
    }
}