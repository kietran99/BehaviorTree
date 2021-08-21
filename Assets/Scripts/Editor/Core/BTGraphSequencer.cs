namespace RR.AI.BehaviorTree
{
    public class BTGraphSequencer : IBTGraphNodeInfo
    {
        public string Name => "Sequencer";

        public BTNodeType NodeType => BTNodeType.Sequencer;
        
        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.Multi);
    }
}
