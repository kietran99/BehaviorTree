using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTComposite : BTBaseNode
    {
        private BTBaseNode[] _children;
        private BTBaseNode _runningLeaf;
        private int _runningLeafIdx;

        protected abstract BTNodeState FinalReturnState { get; }

        public override bool IsLeaf => false;

        protected BTComposite(string guid) : base(guid)
        {}

        public override bool Init(BTBaseNode[] children, GameObject actor, RuntimeBlackboard blackboard)
        {
			if (children.Length == 0)
			{
				return false;
			}

			_children = children;

            return true;
        }

        public override BTNodeState InternalUpdate(GameObject actor, RuntimeBlackboard blackboard, out BTBaseNode runningNode)
        {
            OnTick?.Invoke(_guid);

            if (_runningLeaf != null)
            {
                var res = _runningLeaf.InternalUpdate(actor, blackboard, out var _);

                if (res == BTNodeState.RUNNING)
                {
                    runningNode = this;
                    return res;
                }

                _runningLeaf = null;
                return TickChildren(_runningLeafIdx + 1, actor, blackboard, out runningNode);
            }

            return TickChildren(0, actor, blackboard, out runningNode);
        }

        private BTNodeState TickChildren(int startIdx, GameObject actor, RuntimeBlackboard blackboard, out BTBaseNode runningNode)
        {
            runningNode = null;

            for (int i = startIdx; i < _children.Length; i++)
            {
                var res = _children[i].InternalUpdate(actor, blackboard, out var runningChild);
    
                if (res == FinalReturnState)
                {
                    continue;
                }

                if (res != BTNodeState.RUNNING)
                {
                    return res;
                }

                var isLeafChild = _children[i].IsLeaf;

                if (isLeafChild)
                {
                    _runningLeaf = runningChild;
                    _runningLeafIdx = i;
                }

                runningNode = isLeafChild ? this : runningChild;
                return res;
            }

            return FinalReturnState;
        }

        public override BTNodeState Update(UnityEngine.GameObject actor, RuntimeBlackboard blackboard)
        {
            OnTick?.Invoke(_guid);

            for (int i = 0; i < _children.Length; i++)
            {
                var res = _children[i].Update(actor, blackboard);
                
                if (res != FinalReturnState)
                {
                    return res;
                }
            }

            return FinalReturnState;
        }
    }
}