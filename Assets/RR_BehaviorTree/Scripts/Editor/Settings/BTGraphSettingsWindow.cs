using UnityEditor;
using UnityEngine;

using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTGraphSettingsWindow : EditorWindow
    {
        private TextAsset _nodeIconSettingsAsset;

        private NodeIconSettings[] _nodeIconSettingsList;

        public void Init(TextAsset nodeIconSettingsAsset)
        {
            _nodeIconSettingsAsset = nodeIconSettingsAsset;

            Serialization
                .JsonWrapper
                .ReadJsonArray<NodeIconSettings>(GetIconSettingsAssetPath(_nodeIconSettingsAsset), out _nodeIconSettingsList);
        }

        private void OnGUI()
        {
            ShowIconSettings();
            EditorGUILayout.Space();
            CreateActionButtons();
        }

        private void ShowIconSettings()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField(_nodeIconSettingsAsset, typeof(TextAsset), false);
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (_nodeIconSettingsAsset == null)
            {
                return;
            }

            for (int i = 0; i < _nodeIconSettingsList.Length; i++)
            {
                var setting = _nodeIconSettingsList[i];

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextField(setting.taskname);
                GUI.enabled = true;

                EditorGUI.BeginChangeCheck();
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(setting.icon);
                Object textureAsset = EditorGUILayout.ObjectField(icon, typeof(Texture2D), false);
                
                if (EditorGUI.EndChangeCheck())
                {
                    _nodeIconSettingsList[i].icon = AssetDatabase.GetAssetPath(textureAsset);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void CreateActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Update"))
            {
                _nodeIconSettingsList = UpdateIconSettings(); 
            }

            if (GUILayout.Button("Save"))
            {
                Serialization.JsonWrapper.OverwriteJsonArray<NodeIconSettings>(GetIconSettingsAssetPath(_nodeIconSettingsAsset), _nodeIconSettingsList);
            }

            EditorGUILayout.EndHorizontal();
        }

        private NodeIconSettings[] UpdateIconSettings()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            System.Func<NodeIconSettings[], string, NodeIconSettings> CreateSettings = (settings, taskName) =>
            {
                var icon = settings.FirstOrDefault(setting => setting.taskname == taskName).icon;
                return new NodeIconSettings(){ taskname = taskName, icon = icon };
            };

            var nodeIconSettingList = new System.Collections.Generic.List<NodeIconSettings>(3)
            {
                CreateSettings(_nodeIconSettingsList, nameof(BTGraphRoot)),
                CreateSettings(_nodeIconSettingsList, nameof(BTGraphSelector)),
                CreateSettings(_nodeIconSettingsList, nameof(BTGraphSequencer)),
            };

            foreach (var assembly in assemblies)
            {

                var taskIconSettingList = assembly.GetTypes()
                                .Where(type => typeof(BTBaseTask).IsAssignableFrom(type) 
                                                && type != typeof(BTBaseTask)
                                                && !type.IsGenericType
                                                && type != typeof(BTTaskNull))
                                                .Select(taskType => CreateSettings(_nodeIconSettingsList, taskType.Name)); 

                nodeIconSettingList.AddRange(taskIconSettingList); 
            }

            return nodeIconSettingList.ToArray();
        }

        private string GetIconSettingsAssetPath(TextAsset iconSettings) => AssetDatabase.GetAssetPath(iconSettings);
    }
}
