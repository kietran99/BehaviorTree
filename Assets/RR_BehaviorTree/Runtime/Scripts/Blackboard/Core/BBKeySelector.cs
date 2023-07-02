using UnityEngine;

using System;

namespace RR.AI
{
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
            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogWarning("Empty key, return a default instead");
            }

            return string.IsNullOrEmpty(_key) ? default(T) : blackboard.GetValue<T>(_key);
        }

        public bool UpdateValue(RuntimeBlackboard blackboard, T newValue)
        {
            return string.IsNullOrEmpty(_key) ? false : blackboard.UpdateEntry<T>(_key, newValue);
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
