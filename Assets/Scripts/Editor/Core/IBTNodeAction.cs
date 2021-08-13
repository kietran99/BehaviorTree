namespace RR.AI.BehaviorTree
{
    public interface IBTNodeAction
    {
        string Name { get; }
        BTNodeType NodeType { get; }
        (BTPortCapacity In, BTPortCapacity Out) Capacity { get; } 
        void Tick(UnityEngine.GameObject actor, UnityEditor.Experimental.GraphView.Blackboard bLackboard);
    }
}