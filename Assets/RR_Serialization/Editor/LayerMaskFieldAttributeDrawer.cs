using UnityEngine;
using UnityEditor;

namespace RR.Serialization
{
	[CustomPropertyDrawer(typeof(LayerMaskFieldAttribute))]
	public class LayerMaskFieldAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (fieldInfo.FieldType != typeof(int))
			{
				EditorGUI.LabelField(position, label.text, "Use LayerField with int only.");
				return;
			}

			var allLayersName = GetAllLayersName();
			property.intValue = EditorGUI.MaskField(position, label, property.intValue, allLayersName);
		}

		private string[] GetAllLayersName()
		{
			System.Collections.Generic.List<string> layerNames = new System.Collections.Generic.List<string>();

			for (int i = 0; i <= 31 ; i++)
			{
				var layer = LayerMask.LayerToName(i);

				if (!string.IsNullOrEmpty(layer))
				{
					layerNames.Add(layer);
				}
			}

			return layerNames.ToArray();
		}
	}
}