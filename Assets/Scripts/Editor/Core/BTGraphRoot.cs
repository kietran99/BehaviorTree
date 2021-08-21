namespace RR.AI.BehaviorTree
{
    public class BTGraphRoot : IBTGraphNodeInfo
    {
        public string Name => "Root";

        public BTNodeType NodeType => BTNodeType.Root;

        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.None, BTPortCapacity.Single);
    }
}
