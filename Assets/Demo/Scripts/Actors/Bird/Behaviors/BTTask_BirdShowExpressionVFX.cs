using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTask_BirdShowExpressionVFX : BTTaskBase
    {
        public override string Name => "Show Expression VFX";

        public enum Expression
        {
            Heart,
            Exclamation,
        }

        [SerializeField]
        private Expression _expression = Expression.Heart;

        private BirdVFXExpression _expressionVFX;

        protected override void OnStart()
        {
            _expressionVFX = _actor.GetComponentInChildren<BirdVFXExpression>(true);
        }

        protected override void OnEnter()
        {
            switch (_expression)
            {
                case Expression.Heart:
                    _expressionVFX.ShowHeart();
                    break;
                case Expression.Exclamation:
                    _expressionVFX.ShowExclamation();
                    break;
                default:
                    break;
            }
        }

        protected override BTNodeState OnUpdate()
        {
            return _expressionVFX.IsAnimFinished() ? BTNodeState.Success : BTNodeState.Running;
        }
    }
}
