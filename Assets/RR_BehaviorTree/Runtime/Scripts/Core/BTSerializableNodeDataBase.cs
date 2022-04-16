using UnityEngine;

namespace RR.AI.BehaviorTree
{
	[System.Serializable]
    public abstract class BTSerializableNodeDataBase
    {
        [SerializeField]
        private BTNodeGraphData _graphData = null;

        public BTSerializableNodeDataBase(Vector2 position, string name, string description, string guid, string parentGuid)
        {
            _graphData = new BTNodeGraphData(position, name, description, guid, parentGuid);
        }

        public string Guid => _graphData.Guid;

        public string ParentGuid 
        {
            get => _graphData.ParentGuid;
            set => _graphData.ParentGuid = value;
        }
        
        public string Name => _graphData.Name;
        public Vector2 Position 
        {
            get => _graphData.Position;
            set => _graphData.Position = value;
        }

        public string Description => _graphData.Description;
    }

}