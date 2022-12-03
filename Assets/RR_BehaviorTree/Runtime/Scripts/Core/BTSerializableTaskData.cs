using UnityEngine;

namespace RR.AI.BehaviorTree
{
	[System.Serializable]
    public class BTSerializableTaskData : BTSerializableNodeDataBase
    {
        [SerializeField]
        private BTTaskBase _task = null;

        public BTSerializableTaskData(
            Vector2 position, string name, string description, string guid, string parentGuid, BTTaskBase task)
            : base(position, name, description, guid, parentGuid)
        {
            _task = task;
        }

        public BTTaskBase Task => _task;
    }
}