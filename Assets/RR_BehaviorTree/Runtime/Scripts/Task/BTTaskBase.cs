using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTTaskBase : ScriptableObject, IBTLifeCycleReceiver
    {
        protected GameObject _actor;
        protected RuntimeBlackboard _blackboard;

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

        public void Init(GameObject actor, RuntimeBlackboard blackboard)
        {
            _actor = actor;
            _blackboard = blackboard;
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
