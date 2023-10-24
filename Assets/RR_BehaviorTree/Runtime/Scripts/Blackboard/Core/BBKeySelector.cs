using UnityEngine;

using System;

namespace RR.AI
{
    [Serializable]
    public class BBKeySelector<T>
    {
        [SerializeField]
        private string _key = string.Empty;

        [HideInInspector]
        public T typeVar;

        public string Key => _key;

        public static implicit operator string(BBKeySelector<T> keySelector) => keySelector._key;

        public T Value(RuntimeBlackboard blackboard)
        {
            return blackboard.GetValue<T>(_key);
        }

        public bool UpdateValue(RuntimeBlackboard blackboard, T newValue)
        {
            return blackboard.UpdateEntry<T>(_key, newValue);
        }
    }

    [Serializable]
    public class BBKeySelectorObject : BBKeySelector<UnityEngine.Object>
    {      
    }

    [Serializable]
    public class BBKeySelectorVector2 : BBKeySelector<Vector2>
    {      
    }

    [Serializable]
    public class BBKeySelectorBool : BBKeySelector<bool>
    {      
    }
}
