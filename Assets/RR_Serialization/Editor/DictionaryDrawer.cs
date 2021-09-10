using UnityEngine;
using UnityEditor;

namespace RR.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class DictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var entries = property.FindPropertyRelative("Entries");
            // Debug.Log($"Dict Height: {position.height}");

            EditorGUI.PropertyField(position, entries, label, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var entries = property.FindPropertyRelative("Entries");

            if (!entries.isExpanded)
            {
                return base.GetPropertyHeight(property, label);
            }

            var propHeight = base.GetPropertyHeight(property, label);

            if (entries.arraySize == 0)
            {
                return propHeight * 4;
            }

            var arrBaseHeight = propHeight * 3;

            var padding = 4f;
            var value = entries.GetArrayElementAtIndex(0).FindPropertyRelative("Value");
            var hMult = entries.arraySize != 0 ? entries.arraySize : 1;

            if (!value.isArray)
            {
                var nChildren = PropertyDrawerUtility.GetChildrenSize(entries.GetArrayElementAtIndex(0), "Value");
                return arrBaseHeight + hMult * (nChildren * (propHeight + padding) + 2f);
            }

            var accumHeight = 0f;

            foreach (SerializedProperty entry in entries)
            {
                var nArrChildren = entry.FindPropertyRelative("Value").arraySize;
                var childrenHeight = nArrChildren != 0 
                    ? (propHeight + 2f) * 3 + ((propHeight + 2f) * nArrChildren)
                    : propHeight * 4;
                // Debug.Log($"Value Height: {childrenHeight - 4f}");
                accumHeight += childrenHeight - padding;
            }
            
            return arrBaseHeight + accumHeight;
        }
    }
}
