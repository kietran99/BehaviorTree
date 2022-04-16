using UnityEngine;

namespace RR.AI.BehaviorTree
{
	[System.Serializable]
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

        public string ParentGuid 
        {
            get => _parentGuid;
            set => _parentGuid = value;
        }

        public string Name => _name;
        public Vector2 Position 
        {
            get => _position;
            set => _position = value;
        }

        public string Description => _description;    
    }
}