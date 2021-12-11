using UnityEngine;
using UnityEditor;

namespace RR.Serialization
{
	[CustomPropertyDrawer(typeof(TagFieldAttribute))]
	public class TagFieldAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (fieldInfo.FieldType != typeof(string))
			{
				EditorGUI.LabelField(position, label.text, "Use TagField with string only.");
				return;
			}

			property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

			if (string.IsNullOrEmpty(property.stringValue))
			{
				property.stringValue = "Untagged";
			}
		}
	}
}