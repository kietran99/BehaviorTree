namespace RR.AI.BehaviorTree
{
    public class BTGraphInitParamsNodeLeaf : BTGraphInitParamsNode
    {
        public BTBaseTask task;

        public BTGraphInitParamsNodeLeaf()
        {}

        public BTGraphInitParamsNodeLeaf(BTBaseTask task, UnityEngine.Vector2 pos, GraphBlackboard blackboard)
        {
            this.task = task;
            this.pos = pos;
            this.blackboard = blackboard;
            this.name = task.Name;
            this.icon = task.Icon;
        }
    }
}
