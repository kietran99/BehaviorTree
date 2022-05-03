using System.Collections.Generic;
using System;

namespace RR.AI.BehaviorTree
{
    public class BTExecListBuilder<TIn, TOut> 
        where TIn : BTSerializableNodeDataBase
        where TOut : IBTIdentifiable, IBTOrderable
    {
        private Dictionary<string, int> _guidToIdxDict;

        private Action<TOut, string> _onObjCreateCb;
        private Action<TOut, int, string> _onObjOrderCb;

        public BTExecListBuilder()
        {
            _guidToIdxDict = new Dictionary<string, int>();
        }

        public BTExecListBuilder<TIn, TOut> OnObjectCreate(Action<TOut, string> onObjCreateCb)
        {
            _onObjCreateCb = onObjCreateCb;
            return this;
        }

        public BTExecListBuilder<TIn, TOut> OnObjectOrder(Action<TOut, int, string> onObjOrderCb)
        {
            _onObjOrderCb = onObjOrderCb;
            return this;
        }

        public int GetNodeIndex(string guid, int defaultIdx = 0)
        {
            if (_guidToIdxDict.TryGetValue(guid, out int idx))
            {
                return idx;
            }
            
            return defaultIdx;
        }

        public List<TOut> Execute(List<TIn> nodeDataList, Func<TIn, TOut> createObjCb)
        {
            return CreateOrderedList(nodeDataList, createObjCb);
        }

        private List<TOut> CreateOrderedList(List<TIn> nodeDataList, Func<TIn, TOut> createObjCb)
        {
            Dictionary<string, List<TOut>> adjacentDict = MakeAdjacentDict(nodeDataList, createObjCb);
            var orderedList = new List<TOut>(nodeDataList.Count);
            var traverseStack = new Stack<(TOut node, string nextSiblingGuid)>();
            TOut root = adjacentDict[string.Empty][0];
            traverseStack.Push((root, string.Empty));
            int curIdx = 0;

            while (traverseStack.Count > 0)
            {
                (TOut curNode, string curNextSiblingGuid) = traverseStack.Pop();

                orderedList.Add(curNode);
                _onObjOrderCb?.Invoke(curNode, curIdx, curNextSiblingGuid);
                _guidToIdxDict[curNode.Guid] = curIdx;
                ++curIdx;

                var isLeafNode = !adjacentDict.ContainsKey(curNode.Guid);

                if (isLeafNode)
                {
                    continue;
                }

                List<TOut> children = adjacentDict[curNode.Guid];
                children.Sort((thisObj, thatObj) => thisObj.y.CompareTo(thatObj.y));

                int nChildren = children.Count;
                for (int i = nChildren - 1; i >= 0; i--)
                {
                    string nextSiblingGuid = i == nChildren - 1 ? string.Empty : children[i + 1].Guid;
                    traverseStack.Push((children[i], nextSiblingGuid));
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
