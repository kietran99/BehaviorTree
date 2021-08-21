namespace RR.AI.BehaviorTree
{
    public class BTGraphSelector : IBTGraphNodeInfo
    {
        public string Name => "Selector";

        public BTNodeType NodeType => BTNodeType.Selector;
        
        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.Multi);
    }
}
