using UnityEditor;
using UnityEngine;

namespace RR.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
    public class KeyValuePairDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   
            const float PADDING = 4f;
            var propHeight = base.GetPropertyHeight(property, label);
            var value = property.FindPropertyRelative("Value");

            if (value.isArray)
            {
                var nArrChildren = value.arraySize;
                return nArrChildren == 0 ? propHeight * 4 : propHeight * 3 + ((propHeight + 2f) * nArrChildren);
            }

            var nChildren = PropertyDrawerUtility.GetChildrenSize(property, "Value");
            return (propHeight + PADDING) * nChildren;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Debug.Log(position.height);
            EditorGUI.BeginProperty(position, label, property);

            var key = property.FindPropertyRelative("Key");
            var value = property.FindPropertyRelative("Value");

            var keyWidth = position.width * .4f;
            const float VALUE_OFFSET = 15f;
            const float PADDING = 2f;
            var propHeight = EditorGUIUtility.singleLineHeight;
            var childrenSize = PropertyDrawerUtility.GetChildrenSize(property, "Value");
            var keyRect = new Rect(position.x, position.y + PADDING, keyWidth, propHeight);
            var valueRect = new Rect(
                position.x + keyWidth + VALUE_OFFSET, 
                position.y + PADDING, 
                position.width - keyWidth - VALUE_OFFSET, 
                position.height);

            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 15f;
            EditorGUI.PropertyField(keyRect, key, new GUIContent(Resources.Load<Texture>("Icons/key")));
            EditorGUIUtility.labelWidth = originalLabelWidth;
            
            DrawChildrenProps(value, valueRect, propHeight, PADDING);

            EditorGUI.EndProperty();
        }

        private void DrawChildrenProps(SerializedProperty prop, Rect baseRect, float propHeight, float padding)
        {
            if (!prop.hasChildren)
            {
                var rect = new Rect(baseRect.x, baseRect.y, baseRect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(rect, prop, GUIContent.none);
                return;
            }

            if (prop.isArray)
            {
                var hVal = prop.arraySize != 0
                    ? EditorGUIUtility.singleLineHeight * (3 + prop.arraySize)
                    : EditorGUIUtility.singleLineHeight * 4;
                var rect = new Rect(baseRect.x, baseRect.y, baseRect.width, hVal);
                EditorGUI.PropertyField(rect, prop, new GUIContent("Value"), true);
                return;
            }

            var nextRect = baseRect;
            nextRect.height = propHeight;

            foreach (SerializedProperty child in prop)
            {
                EditorGUI.PropertyField(nextRect, child, GUIContent.none);
                nextRect.y += propHeight + padding;
            }       
        }
    }
}
