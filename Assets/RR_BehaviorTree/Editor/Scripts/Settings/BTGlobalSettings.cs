using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTGlobalSettings : ScriptableObject
    {
        const string DEFAULT_SETTINGS_PATH = "Assets/Resources/BT_Settings/";

        [SerializeField]
        private TextAsset _nodeIconSettingsAsset = null;

        [SerializeField]
        private bool _playgroundMode = true;

#region SINGLETON
        private static BTGlobalSettings _instance;

        public static BTGlobalSettings Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                System.Func<string, BTGlobalSettings> createAssetFn = path =>
                {
                    var globalSettingsAsset = CreateInstance<BTGlobalSettings>();
                    Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(globalSettingsAsset, path + $"{ nameof(BTGlobalSettings) }.asset");
                    AssetDatabase.Refresh();
                    return globalSettingsAsset;
                };

                _instance = FindOrCreateAsset<BTGlobalSettings>(createAssetFn, DEFAULT_SETTINGS_PATH);
                return _instance;
            }
        }
#endregion

        private NodeIconSettingsManager _nodeIconSettingsManager;


        public bool PlaygroundMode => _playgroundMode;

        public TextAsset NodeIconSettingsAsset
        {
            get
            {
                if (_nodeIconSettingsAsset != null)
                {
                    return _nodeIconSettingsAsset;
                }

                System.Func<string, TextAsset> createAssetFn = path =>
                {
                    string destAssetPath = path + NodeIconSettingsManager.DEF_SETTINGS_ASSET_NAME;
                    
                    if (!File.Exists(destAssetPath))
                    {
                        var templatePath = NodeIconSettingsManager.FindTemplateJsonPath();
                        Debug.Log(templatePath);

                        if (string.IsNullOrEmpty(templatePath))
                        {
                            return null;
                        }

                        File.Copy(templatePath, destAssetPath);
                        AssetDatabase.Refresh();
                    }

                    return AssetDatabase.LoadAssetAtPath<TextAsset>(destAssetPath);
                };

                _nodeIconSettingsAsset = FindOrCreateAsset<TextAsset>(
                    createAssetFn, DEFAULT_SETTINGS_PATH, NodeIconSettingsManager.DEF_SETTINGS_ASSET_NAME);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
                return _nodeIconSettingsAsset;
            }
        }

        public Texture2D GetIcon(string nodeTypeName)
        {
            if (_nodeIconSettingsManager == null)
            {
                _nodeIconSettingsManager = new NodeIconSettingsManager(NodeIconSettingsAsset);
            }

            return _nodeIconSettingsManager.GetIcon(nodeTypeName);
        }

        private static T FindOrCreateAsset<T>(System.Func<string, T> createAssetFn, string path, string name="") where T : Object
        {
            T assetToFind = FindAsset<T>(name);
            return assetToFind == null ? createAssetFn(path) : assetToFind;
        }

        private static T FindAsset<T>(string name="") where T : Object
        {
            string assetFilter = string.IsNullOrEmpty(name) ? $"t:{ nameof(T) }" : name;
            string[] possibleSettingsGUIDs = AssetDatabase.FindAssets(assetFilter, new[] { "Assets" });

            if (possibleSettingsGUIDs.Length > 0)
            {
                string guid = possibleSettingsGUIDs[0];
                string settingsPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(settingsPath);
                return asset;
            }

            return null;
        }

        private class NodeIconSettingsManager
        {
            public const string DEF_SETTINGS_ASSET_NAME = "node-icon-settings.json";
            public const string DEF_SETTINGS_TEMPLATE_NAME = "node-icon-settings-template";
            
            private Dictionary<string, string> _nodeIconSettingsDict;

            public NodeIconSettingsManager(TextAsset nodeIconSettingsAsset)
            {
                _nodeIconSettingsDict = CreateNodeIconDict(nodeIconSettingsAsset);
            }

            public static string FindTemplateJsonPath()
            {
                string[] templateGuidList = AssetDatabase.FindAssets($"{DEF_SETTINGS_TEMPLATE_NAME} t:TextAsset");

                foreach (var templateGuid in templateGuidList)
                {
                    return AssetDatabase.GUIDToAssetPath(templateGuid);
                }

                return string.Empty;
            }

            public Texture2D GetIcon(string nodeTypeName)
            {
                if (!_nodeIconSettingsDict.ContainsKey(nodeTypeName))
                {
                    return null;
                }

                return Resources.Load<Texture2D>(_nodeIconSettingsDict[nodeTypeName]);
            }

            private Dictionary<string, string> CreateNodeIconDict(TextAsset nodeIconSettingsAsset)
            { 
                if (_nodeIconSettingsDict != null)
                {
                    return _nodeIconSettingsDict;
                }

                Serialization
                    .JsonWrapper
                    .ReadJsonArray<BTSettingsNodeIconItem>(
                        AssetDatabase.GetAssetPath(nodeIconSettingsAsset), 
                        out BTSettingsNodeIconItem[] nodeIconSettingsList);

                _nodeIconSettingsDict = new Dictionary<string, string>(nodeIconSettingsList.Length);
                foreach (BTSettingsNodeIconItem settings in nodeIconSettingsList)
                {
                    _nodeIconSettingsDict.Add(settings.taskname, settings.icon);
                }

                return _nodeIconSettingsDict;
            }
        }
    }
}
