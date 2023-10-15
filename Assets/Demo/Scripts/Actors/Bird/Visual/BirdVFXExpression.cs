using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BirdVFXExpression : MonoBehaviour
    {
        [SerializeField]
        private BirdVFXAnimEventHandler _vfxAnimEventHandler = null;

        [SerializeField]
        private GameObject _background = null;

        [SerializeField]
        private Animator _animator = null;

        private GameObject _expressionPlayer = null;

        private void Start()
        {
            _vfxAnimEventHandler.AnimFinished += OnAnimFinished;
            _expressionPlayer = _vfxAnimEventHandler.gameObject;
        }

        public void ShowHeart()
        {
            _background.SetActive(true);
            _expressionPlayer.SetActive(true);
            _animator.Play("A_VFX_Heart");
        }

        public void ShowExclamation()
        {
            _background.SetActive(true);
            _expressionPlayer.SetActive(true);
            _animator.Play("A_VFX_Exclamation");
        }

        public bool IsAnimFinished()
        {
            return !_expressionPlayer.activeSelf;
        }

        private void OnAnimFinished()
        {
            _background.SetActive(false);
            _expressionPlayer.SetActive(false);
        }
    }
}
