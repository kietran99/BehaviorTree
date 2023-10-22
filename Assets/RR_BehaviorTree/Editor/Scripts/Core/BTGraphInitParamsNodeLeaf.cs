namespace RR.AI.BehaviorTree
{
    public class BTGraphInitParamsNodeLeaf : BTGraphInitParamsNode
    {
        public BTTaskBase task;

        public BTGraphInitParamsNodeLeaf()
        {}

        public BTGraphInitParamsNodeLeaf(BTTaskBase task, UnityEngine.Vector2 pos, GraphBlackboard blackboard)
        {
            this.task = task;
            this.pos = pos;
            this.blackboard = blackboard;
            this.name = task.Name;
            this.icon = BTGlobalSettings.Instance.GetIcon(task.GetType());
        }
    }
}
