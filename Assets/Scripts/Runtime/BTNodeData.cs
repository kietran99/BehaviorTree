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
        private Vector2 _position = Vector2.zero;

        public BTNodeGraphData(Vector2 position, string guid, string parentGuid)
        {
            _guid = guid;
            _parentGuid = parentGuid;
            _position = position;
        }

        public string Guid => _guid;
        public string ParentGuid => _parentGuid;
        public Vector2 Position => _position;        
    }

    [Serializable]
    public class BTNodeData
    {
        [SerializeField]
        private BTNodeGraphData _graphData = null;

        [SerializeField]
        private BTNodeType _nodeType = BTNodeType.Root;

        public BTNodeData(Vector2 position, string guid, string parentGuid, BTNodeType nodeType)
        {
            _graphData = new BTNodeGraphData(position, guid, parentGuid);
            _nodeType = nodeType;
        }

        public string Guid => _graphData.Guid;
        public string ParentGuid => _graphData.ParentGuid;
        public Vector2 Position => _graphData.Position;
        public BTNodeType NodeType => _nodeType;
    }

    [Serializable]
    public class BTTaskData
    {
        [SerializeField]
        private BTNodeGraphData _graphData = null;

        [SerializeField]
        private BTBaseTask _task = null;

        public BTTaskData(Vector2 position, string guid, string parentGuid, BTBaseTask task)
        {
            _graphData = new BTNodeGraphData(position, guid, parentGuid);
            _task = task;
        }

        public string Guid => _graphData.Guid;
        public string ParentGuid => _graphData.ParentGuid;
        public Vector2 Position => _graphData.Position;
        public BTBaseTask Task => _task;
    }
}