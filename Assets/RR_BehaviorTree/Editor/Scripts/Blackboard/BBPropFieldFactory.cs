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

            // VisualElement x = valueType switch
            // {
            //     var type when type == typeof(int) => Create((int)valueContainer.ValueAsObject),
            //     var type when type == typeof(float) => Create((float)valueContainer.ValueAsObject),
            //     var type when type == typeof(bool) => Create((bool)valueContainer.ValueAsObject),
            //     var type when type == typeof(string) => Create((string)valueContainer.ValueAsObject),
            //     var type when type == typeof(Vector2) => Create((Vector2)valueContainer.ValueAsObject),
            //     var type when type == typeof(Vector3) => Create((Vector3)valueContainer.ValueAsObject),
            //     var type when type == typeof(Object) => Create((Object)valueContainer.ValueAsObject),
            //     _ => null
            // };

            switch (valueType)
            {
                case var type when type == typeof(int):
                    return Create((int)valueContainer.ValueAsObject);
                case var type when type == typeof(float):
                    return Create((float)valueContainer.ValueAsObject);
                case var type when type == typeof(bool):
                    return Create((bool)valueContainer.ValueAsObject);
                case var type when type == typeof(string):
                    return Create((string)valueContainer.ValueAsObject);
                case var type when type == typeof(Vector2):
                    return Create((Vector2)valueContainer.ValueAsObject);
                case var type when type == typeof(Vector3):
                    return Create((Vector3)valueContainer.ValueAsObject);
                case var type when type == typeof(Object):
                    return Create((Object)valueContainer.ValueAsObject);
                default:
                    return null;
            }
        }

        private static BaseField<T> Create<T>(BaseField<T> field, T value)
        {
            field.value = value;
            return field;
        }
    }
}
