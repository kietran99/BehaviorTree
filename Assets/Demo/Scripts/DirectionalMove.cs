using UnityEngine;

using System;

namespace RR.AI.BehaviorTree
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalMove : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 10f)]
        private float _moveSpeed = 5f;

        [SerializeField]
        private float _invisibleSpeed = 1.0f;

        private Transform _transform;
        private SpriteRenderer _renderer;
        private Vector3 _moveVec;
        private Vector3 _target;
        private bool _startInvisible;

        void Start()
        {
            _transform = transform;
        }

        void Update()
        {
            if (!_startInvisible && (_target - _transform.position).sqrMagnitude <= 0.1f)
            {
                _startInvisible = true;
                return;
            }

            _transform.Translate(_moveSpeed * Time.deltaTime * _moveVec.normalized);

            if (!_startInvisible)
            {
                return;
            }

            Color curColor = _renderer.color;
            var nextColor = new Color(curColor.r, curColor.g, curColor.b, curColor.a - _invisibleSpeed * Time.deltaTime);
            _renderer.color = nextColor;

            if (nextColor.a <= 0.0f)
            {
                Destroy(gameObject);
            }
        }

        public DirectionalMove Towards(Vector3 target)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }
            
            _target = target;
            _moveVec = (_target - transform.position).normalized;
            _startInvisible = false;
            _renderer.flipX = target.x > transform.position.x;
            return this;
        }
    }
}
