using UnityEditor;
using UnityEngine;

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
