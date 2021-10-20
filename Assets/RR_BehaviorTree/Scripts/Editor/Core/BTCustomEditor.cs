using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    [CustomEditor(typeof(BehaviorTree))]
    public class BTCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Editor"))
            {
                BTEditorWindow.Init(target as BehaviorTree);
            }
        }
    }
}
