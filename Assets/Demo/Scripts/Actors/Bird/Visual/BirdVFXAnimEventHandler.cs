using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BirdVFXAnimEventHandler : MonoBehaviour
    {
        public System.Action AnimFinished { get; set; }        

        public void OnFinish()
        {
            AnimFinished?.Invoke();
        }
    }
}
