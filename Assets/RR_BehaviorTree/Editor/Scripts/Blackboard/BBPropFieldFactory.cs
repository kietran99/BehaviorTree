using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI
{
    public static class BBPropFieldFactory
    {
        public static BaseField<int> Create(int value) => Create(new IntegerField(), value);

        public static BaseField<float> Create(float value) => Create(new FloatField(), value);

        public static BaseField<bool> Create(bool value) => Create(new Toggle(), value);

        public static BaseField<string> Create(string value) => Create(new TextField(), value);

        public static BaseField<Vector2> Create(Vector2 value) => Create(new Vector2Field(), value);

        public static BaseField<Vector3> Create(Vector3 value) => Create(new Vector3Field(), value);

        public static BaseField<Object> Create(Object value) => Create(new ObjectField() { objectType = typeof(Object) }, value);

        public static ObjectField Create<T>(T value) where T : Object => new ObjectField() { objectType = typeof(T), value = value };

        public static VisualElement Create(IBBValueBase valueContainer)
        {
            System.Type valueType = valueContainer.ValueType;

            if (valueType == typeof(int))
            {
                return Create((int)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(float))
            {
                return Create((float)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(bool))
            {
                return Create((bool)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(string))
            {
                return Create((string)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(Vector2))
            {
                return Create((Vector2)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(Vector3))
            {
                return Create((Vector3)valueContainer.ValueAsObject);
            }
            else if (valueType == typeof(Object))
            {
                return Create((Object)valueContainer.ValueAsObject);
            }

            return null;
        }

        private static BaseField<T> Create<T>(BaseField<T> field, T value)
        {
            field.value = value;
            return field;
        }
    }
}
