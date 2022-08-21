using UnityEngine;

namespace RR.AI
{
    public interface IBBSerializableValue : IBBValueBase
    {
        string ValueTypeString { get; }

        bool SetValue(object value);
        bool AddToRuntimeBlackboard(RuntimeBlackboard runtimeBB, string key);
    }

    public abstract class BBSerializableValue<T> : ScriptableObject, IBBSerializableValue
    {
        [SerializeField]
        private T _value;

        public T Value { get => _value; set => _value = value; }
        public object ValueAsObject => _value as object;
        public System.Type ValueType => typeof(T);

        public abstract string ValueTypeString { get; }
         
        public bool SetValue(object value)
        {
            if (!(value is T))
            {
                return false;
            }

            Value = (T) value;
            return true;
        }

        public bool AddToRuntimeBlackboard(RuntimeBlackboard runtimeBB, string key) => runtimeBB.Add(key, _value);
    }
}
