using UnityEngine;

using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTDecoBBBasedCondition : BTDecoratorBase
    {
        [SerializeField]
        private KeyQuery _keyQuery = KeyQuery.IsSet;

        [SerializeField]
        private BBKeySelectorObject _blackboardKey = null;

        public override string Name => "Blackboard Based Condition";

        public static List<string> KeyQueryOptions()
        {
            var result = new List<string>(System.Enum.GetNames(typeof(KeyQuery)));
            return result;
        }

        private enum KeyQuery
        {
            IsSet,
            IsNotSet
        }

        private void OnBBEntryUpdated(BBUpdateEntryEvent<Object> evt)
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
            Object target = _blackboardKey.Value(_blackboard);
            bool hasMetCondition = HasMetCondition(target);
            TryObserveBlackboardEntry(hasMetCondition);
            return hasMetCondition.ToBTDecoState();
        }

        protected override void OnTreeEval()
        {
            BBEventBroker.Instance.Unsubscribe<BBUpdateEntryEvent<Object>>(OnBBEntryUpdated);
        }

        private bool HasMetCondition(Object target)
        {
            return (_keyQuery == KeyQuery.IsSet && target != null) || (_keyQuery == KeyQuery.IsNotSet && target == null);
        }

        private bool TryObserveBlackboardEntry(bool hasMetCondition)
        {
            if (_observerAborts == ObserverAborts.Both
                || (hasMetCondition && _observerAborts == ObserverAborts.Self)
                || (!hasMetCondition && _observerAborts == ObserverAborts.LowerPriority))
            {
                SubScribeBlackboardEntryUpdateEvt();
                return true;
            }

            return false;
        }

        private void SubScribeBlackboardEntryUpdateEvt() => BBEventBroker.Instance.Subscribe<BBUpdateEntryEvent<Object>>(OnBBEntryUpdated);
    }
}
