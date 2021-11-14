using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public static class BTGraphNodeFactory
    {
        private static Dictionary<System.Type, System.Type> _nodeInfoToGraphNodeMap = new Dictionary<System.Type, System.Type>();

        public static Node CreateDefaultGraphNode(
            System.Type graphInfoType
            , GraphBlackboard blackboard
            , Vector2 pos
            , System.Func<System.Type, BTBaseTask> GetOrCreateTaskFn 
            )
        {
            var isTaskNode = typeof(BTBaseTask).IsAssignableFrom(graphInfoType);

            var ctorParams = isTaskNode
                ? new object[] { GetOrCreateTaskFn(graphInfoType), pos, blackboard, string.Empty, string.Empty, string.Empty }
                : new object[] { pos, blackboard, string.Empty, string.Empty, string.Empty, null };
            
            if (!_nodeInfoToGraphNodeMap.TryGetValue(graphInfoType, out var graphNodeType))
            {
                graphNodeType = isTaskNode
                    ? typeof(BTGraphNodeLeaf<>).MakeGenericType(graphInfoType)
                    : typeof(BTGraphNode<>).MakeGenericType(graphInfoType);
                _nodeInfoToGraphNodeMap.Add(graphInfoType, graphNodeType);
            }

            var node = System.Activator.CreateInstance(graphNodeType, ctorParams);
            return node as Node;
        }

        public static Node CreateGraphNode(BTNodeType nodeType, Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid="") 
        {
            switch (nodeType)
            {
                case BTNodeType.Root:
                    return new BTGraphNode<BTGraphRoot>(pos, blackboard, name, desc, guid);
                case BTNodeType.Sequencer:
                    return new BTGraphNode<BTGraphSequencer>(pos, blackboard, name, desc, guid);
                case BTNodeType.Selector:
                    return new BTGraphNode<BTGraphSelector>(pos, blackboard, name, desc, guid);
                default:
                    return new BTGraphNode<BTGraphRoot>(pos, blackboard, name, desc, guid);
            }
        }

        public static Node CreateGraphNodeLeaf(
            BTBaseTask task, Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid="")
        {
            var graphNodeleafType = typeof(BTGraphNodeLeaf<>).MakeGenericType(task.GetType());
            return System.Activator.CreateInstance(graphNodeleafType, new object[] { pos, blackboard, name, desc, guid }) as Node;
        }
    }
}
