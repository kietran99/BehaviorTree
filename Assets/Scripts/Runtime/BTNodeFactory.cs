namespace RR.AI.BehaviorTree
{
    public static class BTNodeFactory
    {
        public static IBTNode Create(BTNodeType type)
        {
            switch(type)
            {
                case BTNodeType.Root:
                    return new BTRoot();
                case BTNodeType.Sequencer:
                    return new BTSequencer();
                default:
                    return new BTRoot();
            }
        }

        public static IBTNode CreateLeaf(BTBaseTask task)
        {
            return new BTLeaf(task);
        }
    }
}