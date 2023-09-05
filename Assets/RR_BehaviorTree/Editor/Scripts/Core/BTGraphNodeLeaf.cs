using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeLeaf<T> : BTGraphNode<BTGraphLeaf<T>> where T : BTTaskBase
    {
        protected override BTTaskBase Task => _nodeAction.Task;

        // Invoked by BTGraphNodeFactory using Reflection
        public BTGraphNodeLeaf(BTGraphInitParamsNodeLeaf initParams) : base(initParams)
        {
            _nodeAction.Task = initParams.task;
        }

        public override void OnCreate(BTGraphDesign graphDesign, Vector2 position)
        {   
            graphDesign.TaskDataList.Add(
                new BTSerializableTaskData(position, 
                NodeName,
                _description,
                _guid, 
                GetParentGuid(inputContainer),
                _nodeAction.Task));
        }

        public override void OnDelete(BTGraphDesign designContainer)
        {
            BTSerializableTaskData nodeToDelete = designContainer.TaskDataList.Find(node => node.Guid == _guid);
            designContainer.TaskDataList.Remove(nodeToDelete);
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(nodeToDelete.Task);
        }

        public override void OnMove(BTGraphDesign designContainer, Vector2 moveDelta)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).Position = GetPosition().position;
        }

        public override void OnConnect(BTGraphDesign designContainer, string parentGuid)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).ParentGuid = parentGuid;
        }

        protected override string MainContentStyleClassName => "task";
    }
}
