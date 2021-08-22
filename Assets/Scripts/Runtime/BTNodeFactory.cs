namespace RR.AI.BehaviorTree
{
    public static class BTNodeFactory
    {
        public static BTBaseNode Create(BTNodeType type, string guid)
        {
            switch(type)
            {
                case BTNodeType.Root:
                    return new BTRoot(guid);
                case BTNodeType.Sequencer:
                    return new BTSequencer(guid);
                case BTNodeType.Selector:
                    return new BTSelector(guid);
                default:
                    return new BTRoot(guid);
            }
        }

        public static BTBaseNode CreateLeaf(BTBaseTask task, string guid)
        {
            return new BTLeaf(task, guid);
        }
    }
}