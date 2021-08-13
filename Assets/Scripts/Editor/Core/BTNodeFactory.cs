using UnityEditor.Experimental.GraphView;

namespace RR.AI.BehaviorTree
{
    public static class BTNodeFactory
    {
        public static Node CreateNodeGeneric<T>(UnityEngine.Vector2 pos, string guid="") 
            where T : IBTNodeAction, new()
        {
            return new BTNode<T>(pos, guid);
        }

        public static Node CreateNode(BTNodeType nodeType, UnityEngine.Vector2 pos, string guid="") 
        {
            switch (nodeType)
            {
                case BTNodeType.Root:
                    return new BTNode<BTRoot>(pos, guid);
                case BTNodeType.Sequencer:
                    return new BTNode<BTSequencer>(pos, guid);
                case BTNodeType.Selector:
                    return new BTNode<BTSelector>(pos, guid);
                default:
                    return new BTNode<BTRoot>(pos, guid);
            }
        }

        public static Node CreateNode(BTBaseTask task, UnityEngine.Vector2 pos, string guid="")
        {
            var leafType = typeof(BTNodeLeaf<>).MakeGenericType(task.GetType());
            return System.Activator.CreateInstance(leafType, new object[] { pos, guid }) as Node;
        }
    }
}
