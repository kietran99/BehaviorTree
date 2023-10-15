using UnityEngine;
using UnityEditor.Experimental.GraphView;

using System;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public static class BTGraphNodeFactory
    {
        private static Dictionary<Type, Type> _nodeInfoToGraphNodeMap = new Dictionary<Type, Type>();

        public static Node CreateDefaultGraphNode(
            Type graphInfoType
            , GraphBlackboard blackboard
            , Vector2 pos
            , Func<Type, BTTaskBase> TaskCtor)
        {
            var isTaskNode = typeof(BTTaskBase).IsAssignableFrom(graphInfoType);

            var ctorParams = isTaskNode
                ? new object[] { new BTGraphInitParamsNodeLeaf(TaskCtor(graphInfoType), pos, blackboard) }
                : new object[] { new BTGraphInitParamsNode() { pos = pos, blackboard = blackboard, icon = BTGlobalSettings.Instance.GetIcon(graphInfoType) } };
            
            if (!_nodeInfoToGraphNodeMap.TryGetValue(graphInfoType, out var graphNodeType))
            {
                graphNodeType = isTaskNode
                    ? typeof(BTGraphNodeLeaf<>).MakeGenericType(graphInfoType)
                    : typeof(BTGraphNode<>).MakeGenericType(graphInfoType);
                _nodeInfoToGraphNodeMap.Add(graphInfoType, graphNodeType);
            }

            var node = Activator.CreateInstance(graphNodeType, ctorParams);
            return node as Node;
        }

        public static BTGraphNodeBase CreateGraphNode(BTNodeType nodeType, BTGraphInitParamsNode initParams) 
        {
            switch (nodeType)
            {
                case BTNodeType.Root:
                    return new BTGraphNode<BTGraphRoot>(initParams);
                case BTNodeType.Sequencer:
                    return new BTGraphNode<BTGraphSequencer>(initParams);
                case BTNodeType.Selector:
                    return new BTGraphNode<BTGraphSelector>(initParams);
                default:
                    return new BTGraphNode<BTGraphRoot>(initParams);
            }
        }

        public static BTGraphNodeBase CreateGraphNodeLeaf(BTGraphInitParamsNodeLeaf initParams)
        {
            var graphNodeleafType = typeof(BTGraphNodeLeaf<>).MakeGenericType(initParams.task.GetType());
            return Activator.CreateInstance(graphNodeleafType, new object[] { initParams }) as BTGraphNodeBase;
        }
    }
}
