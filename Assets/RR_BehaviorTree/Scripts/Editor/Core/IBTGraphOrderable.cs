namespace RR.AI.BehaviorTree
{
    public interface IBTGraphOrderable : IBTOrderable
    {
        BTGraphOrderLabel OrderLabel { get; set; }
        int OrderValue { get; set; }
    }
}
