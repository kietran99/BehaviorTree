namespace RR.AI.BehaviorTree
{
    public class BTRuntimeNodeBase
    {
        public BTRuntimeNodeBase(string guid, int successIdx, int failIdx, BTNodeType type, BTBaseTask task, BTRuntimeDecorator[] decorators)
        {
            Guid = guid;
            SuccessIdx = successIdx;
            FailIdx = failIdx;
            Type = type;
            Task = task;
            Decorators = decorators;
        }

        public string Guid { get; }
        public int SuccessIdx { get; }
        public int FailIdx { get; }
        public BTNodeType Type { get; }
        public BTBaseTask Task { get; }
        public BTRuntimeDecorator[] Decorators { get; set; }
    }
}
