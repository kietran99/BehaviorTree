namespace RR.AI.BehaviorTree
{
	public readonly struct BTNodeData
	{
		public readonly IBTNode Node;
		public readonly IBTNode[] Children;

        public BTNodeData(IBTNode node, IBTNode[] children)
        {
            Node = node;
            Children = children;
        }
    }
}