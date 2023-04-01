using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTTaskBase : ScriptableObject, IBTLifeCycleReceiver
    {
        protected GameObject _actor;
        protected RuntimeBlackboard _blackboard;
        protected Events.IEventHub _eventHub;

        public virtual string Name
        {
            get
            {
                var typeName = GetType().Name;
                var btTaskNamePrefix = "BTTask";
                var extractedTypeName = typeName.StartsWith(btTaskNamePrefix) ? typeName.Substring(btTaskNamePrefix.Length) : typeName;
                return extractedTypeName;
            }
        }

        public void Init(BTRuntimeContext context)
        {
            _actor = context.Actor;
            _blackboard = context.Blackboard;
            _eventHub = context.EventHub;
            OnStart();
        }

        public BTNodeState Update()
        {
            BTNodeState updateRes = OnUpdate();
            return updateRes;
        }

        public void TreeEval()
        {
            OnTreeEval();
        }

        public void Abort()
        {
            OnAbort();
        }

        protected virtual void OnStart() {}
        protected abstract BTNodeState OnUpdate();
        protected virtual void OnTreeEval() {}
        protected virtual void OnAbort() {}
    }
}
