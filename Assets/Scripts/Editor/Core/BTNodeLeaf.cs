using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTNodeLeaf<T> : BTNode<BTLeaf<T>>, IBTSavable where T : BTBaseTask
    {
        public BTNodeLeaf(Vector2 pos, string guid = "") : base(pos, guid)
        {}

        public override void Save(BTDesignContainer designContainer)
        {
            designContainer.taskDataList.Add(
                new BTTaskData(GetPosition().position, 
                _guid, 
                GetParentGuid(inputContainer), 
                _nodeAction.Task));
        }
    }
}
