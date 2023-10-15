using UnityEngine;

using System;

namespace RR.AI.BehaviorTree
{
    [Serializable]
    public class AnimationStatePicker
    {
        [SerializeField]
        private string _stateName = string.Empty;

        [SerializeField]
        private int _layer = 0;

        public string StateName => _stateName;
        public int Layer => _layer;
    }
}
