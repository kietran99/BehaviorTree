namespace RR.AI.BehaviorTree
{
    public class BTGraphLeaf<T> : IBTGraphNodeInfo where T : BTTaskBase
    {
        private BTTaskBase _task;

        public string Name => _task.Name;

        public BTNodeType NodeType => BTNodeType.Leaf;

        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.None);

        public BTTaskBase Task { get => _task; set => _task = value; }

        public BTGraphLeaf()
        {}
    }
}
