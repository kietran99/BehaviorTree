using UnityEditor.Experimental.GraphView;

namespace RR.AI.BehaviorTree
{
    public static class BTGraphNodeFactory
    {
        public static Node CreateNodeGeneric<T>(UnityEngine.Vector2 pos, string guid="") where T : IBTGraphNodeInfo, new()
        {
            return new BTGraphNode<T>(pos, guid);
        }

        public static Node CreateTaskGeneric<T>(UnityEngine.Vector2 pos, string guid="") where T : BTBaseTask
        {
            return new BTGraphNodeLeaf<T>(pos, guid);
        }

        public static Node CreateNode(BTNodeType nodeType, UnityEngine.Vector2 pos, string guid="") 
        {
            switch (nodeType)
            {
                case BTNodeType.Root:
                    return new BTGraphNode<BTGraphRoot>(pos, guid);
                case BTNodeType.Sequencer:
                    return new BTGraphNode<BTGraphSequencer>(pos, guid);
                case BTNodeType.Selector:
                    return new BTGraphNode<BTGraphSelector>(pos, guid);
                default:
                    return new BTGraphNode<BTGraphRoot>(pos, guid);
            }
        }

        public static Node CreateNode(BTBaseTask task, UnityEngine.Vector2 pos, string guid="")
        {
            var leafType = typeof(BTGraphNodeLeaf<>).MakeGenericType(task.GetType());
            return System.Activator.CreateInstance(leafType, new object[] { pos, guid }) as Node;
        }
    }
}
