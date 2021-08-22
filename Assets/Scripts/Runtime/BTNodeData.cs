namespace RR.AI.BehaviorTree
{
	public readonly struct BTNodeData
	{
		public readonly BTBaseNode Node;
		public readonly BTBaseNode[] Children;

        public BTNodeData(BTBaseNode node, BTBaseNode[] children)
        {
            Node = node;
            Children = children;
        }
    }
}