namespace RR.AI.BehaviorTree
{
	public class BTNodeData
	{
		public BTBaseNode Node { get; private set; }
		public BTBaseNode[] Children { get; private set; }

        public BTNodeData(BTBaseNode node, BTBaseNode[] children)
        {
            Node = node;
            Children = children;
        }
    }
}