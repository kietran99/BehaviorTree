using UnityEngine;

using System;

namespace RR.AI.BehaviorTree
{
    public class BTDecoBlackboardConditonBool : BTDecoBlackboardConditionBase<bool>
    {
        public override string Name => "Blackboard Condition Bool";

        [SerializeField]
        private KeyQuery _keyQuery = KeyQuery.IsSet;
        
        private enum KeyQuery
        {
            IsSet,
            IsNotSet
        }

        protected override void OnStart()
        {
            HasMetCondition = target => (_keyQuery == KeyQuery.IsSet && target) || (_keyQuery == KeyQuery.IsNotSet && !target);;
        } 
    }
}