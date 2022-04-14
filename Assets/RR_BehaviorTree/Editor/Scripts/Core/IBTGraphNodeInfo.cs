namespace RR.AI.BehaviorTree
{
    public interface IBTGraphNodeInfo
    {
        string Name { get; }
        BTNodeType NodeType { get; }
        (BTPortCapacity In, BTPortCapacity Out) Capacity { get; } 
    }
}