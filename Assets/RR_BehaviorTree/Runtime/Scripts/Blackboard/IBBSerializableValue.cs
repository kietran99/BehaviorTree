using UnityEngine;

using System;

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


    [Serializable]
    public class BBInt : BBSerializableValue<int>
    {
        public override string ValueTypeString => "Integer";
    }

    [Serializable]
    public class BBFloat : BBSerializableValue<float>
    {
        public override string ValueTypeString => "Float";
    }

    [Serializable]
	public class BBBool : BBSerializableValue<bool>
	{
		public override string ValueTypeString => "Boolean";
    }

    [Serializable]
    public class BBString : BBSerializableValue<string>
    {
        public override string ValueTypeString => "String";
    }

    [Serializable]
    public class BBVector2 : BBSerializableValue<Vector2>
    {
        public override string ValueTypeString => "Vector2";
    }

    [Serializable]
    public class BBVector3 : BBSerializableValue<Vector3>
    {
        public override string ValueTypeString => "Vector3";
    }

    [Serializable]
    public class BBObject : BBSerializableValue<UnityEngine.Object>
    {
        public override string ValueTypeString => "Object";
    }
}
