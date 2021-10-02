using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI
{
    public interface IBBValue
    {
        System.Type ValueType { get; }
        string ValueTypeString { get; }

        bool SetValue(object value);
        VisualElement CreatePropView();
    }

    // [Serializable]
    public abstract class BBValue<T> : ScriptableObject, IBBValue
    {
        [SerializeField]
        private T _value;

        public T Value { get => _value; set => _value = value; }
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

        public virtual VisualElement CreatePropView()
        {
            var field = PrimitivePropView;
            field.value = Value;
            field.RegisterValueChangedCallback(evt => _value = evt.newValue);
            return field;
        }

        protected abstract BaseField<T> PrimitivePropView { get; }
    }
}