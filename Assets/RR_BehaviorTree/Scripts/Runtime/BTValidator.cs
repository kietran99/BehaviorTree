using System.Collections.Generic;
using System;

namespace RR.AI.BehaviorTree
{
    public class BTValidator<TIn, TOut> 
        where TIn : BTSerializableNodeDataBase
        where TOut : IBTIdentifiable, IBTOrderable
    {
        private Action<TOut, string> _onObjCreateCb;
        private Action<TOut, int> _onObjOrderCb;

        public BTValidator<TIn, TOut> OnObjectCreate(Action<TOut, string> onObjCreateCb)
        {
            _onObjCreateCb = onObjCreateCb;
            return this;
        }

        public BTValidator<TIn, TOut> OnObjectOrder(Action<TOut, int> onObjOrderCb)
        {
            _onObjOrderCb = onObjOrderCb;
            return this;
        }

        public List<TOut> Execute(List<TIn> nodeDataList, Func<TIn, TOut> createObjCb)
        {
            return CreateOrderedList(nodeDataList, createObjCb);
        }

        private List<TOut> CreateOrderedList(List<TIn> nodeDataList, Func<TIn, TOut> createObjCb)
        {
            Dictionary<string, List<TOut>> adjacentDict = MakeAdjacentDict(nodeDataList, createObjCb);
            var orderedList = new List<TOut>(nodeDataList.Count);
            var traverseStack = new Stack<TOut>();
            TOut root = adjacentDict[string.Empty][0];
            traverseStack.Push(root);
            int order = 0;

            while (traverseStack.Count > 0)
            {
                TOut curNode = traverseStack.Pop();

                orderedList.Add(curNode);
                _onObjOrderCb?.Invoke(curNode, order);
                ++order;

                var isLeafNode = !adjacentDict.ContainsKey(curNode.Guid);

                if (isLeafNode)
                {
                    continue;
                }

                List<TOut> children = adjacentDict[curNode.Guid];
                children.Sort((thisObj, thatObj) => thatObj.y.CompareTo(thisObj.y));

                foreach (var child in children)
                {
                    traverseStack.Push(child);
                }
            }

            return orderedList;
        }

        public Dictionary<string, List<TOut>> MakeAdjacentDict(List<TIn> nodeDataList, Func<TIn, TOut> createObjCb)
        {
            int nNodes = nodeDataList.Count;
            var resDict = new Dictionary<string, List<TOut>>(nNodes);

            foreach (var nodeData in nodeDataList)
            {
                TOut item = createObjCb(nodeData);
                string parentGuid = nodeData.ParentGuid;
                AddToOrCreateParentEntry(item, parentGuid, resDict);
                _onObjCreateCb?.Invoke(item, parentGuid);
            }

            return resDict;
        }

        public void AddToOrCreateParentEntry(TOut newNode, string parentID, Dictionary<string, List<TOut>> dict)
        {
            if (!dict.ContainsKey(parentID) || dict[parentID] == null)
            {
                dict[parentID] = new List<TOut>();
            }

            dict[parentID].Add(newNode);
        }
    }
}
