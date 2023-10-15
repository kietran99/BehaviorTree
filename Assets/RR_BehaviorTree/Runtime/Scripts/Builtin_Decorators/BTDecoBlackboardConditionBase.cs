using UnityEngine;

using System;

namespace RR.AI.BehaviorTree
{
    public abstract class BTDecoBlackboardConditionBase<T> : BTDecoratorBase
    {
        [SerializeField]
        private BBKeySelector<T> _blackboardKey = null;

        protected Func<T, bool> HasMetCondition = T => false;

        private void OnBBEntryUpdated(BBUpdateEntryEvent<T> evt)
        {
            if (evt.key != _blackboardKey)
            {
                return;
            }

            // Debug.Log("OnBBEntryUpdated: newValue = " + (evt.newValue == null ? "Null" : evt.newValue) + " oldValue = " + evt.oldValue);
            AbortTriggered?.Invoke(_observerAborts, HasMetCondition(evt.newValue));
        }

        protected override BTDecoState OnEvaluate()
        {
            T target = _blackboardKey.Value(_blackboard);
            bool hasMetCondition = HasMetCondition(target);
            TryObserveBlackboardEntry(hasMetCondition);
            return hasMetCondition.ToBTDecoState();
        }

        protected override void OnTreeEval()
        {
            BBEventBroker.Instance.Unsubscribe<BBUpdateEntryEvent<T>>(OnBBEntryUpdated);
        }

        private bool TryObserveBlackboardEntry(bool _hasMetCondition)
        {
            if (_observerAborts == ObserverAborts.Both
                || (_hasMetCondition && _observerAborts == ObserverAborts.Self)
                || (!_hasMetCondition && _observerAborts == ObserverAborts.LowerPriority))
            {
                SubScribeBlackboardEntryUpdateEvt();
                return true;
            }

            return false;
        }

        private void SubScribeBlackboardEntryUpdateEvt() => BBEventBroker.Instance.Subscribe<BBUpdateEntryEvent<T>>(OnBBEntryUpdated);
    }
}
