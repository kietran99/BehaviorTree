using UnityEngine;

namespace RR.Demo.AI.BehaviorTree
{
    public class CharController : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 10f)]
        private float _moveSpeed = 5f;

        void Update()
        {
            var (moveX, moveY) = (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var realSpeed = Time.deltaTime * _moveSpeed;
            transform.Translate(new Vector3(moveX * realSpeed, moveY * realSpeed, 0f));
        }
    }
}
