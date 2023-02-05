using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public enum ObserverAborts
    {
        None,
        Self,
        LowerPriority,
        Both
    }

    public abstract class BTDecoratorBase : BTDecoratorSimpleBase
    {
        [SerializeField]
        protected ObserverAborts _observerAborts = ObserverAborts.None;

        public System.Action<ObserverAborts, bool> AbortTriggered { get; set; }
    } 
}
