using UnityEditor;

namespace RR.Utils
{
    public class ScriptGeneratorEditorWindow : EditorWindow
    {
        private string _scriptName = string.Empty;
        private bool _isChildClass = false;
        private string _derivesFrom = "MonoBehaviour";
        private bool _inCustomNamespace = false;
        private string _customNamespace = string.Empty;
        private int _nUsingNamespaces = 0;
        private string[] _usingNamespaces = new string[0];

        private string _scriptToGen = string.Empty;

        [MenuItem("Window/Script Generator")]
        public static void Init()
        {
            var window = GetWindow<ScriptGeneratorEditorWindow>("Script Generator");
            window.minSize = new UnityEngine.Vector2(500f, 100f);
            window.maxSize = new UnityEngine.Vector2(700f, 800f);
        }

        private void OnGUI()
        {   
            _scriptName = EditorGUILayout.TextField("Script Name", _scriptName);

            
            _nUsingNamespaces = EditorGUILayout.IntField("Using Namespaces", _nUsingNamespaces);

            if (_usingNamespaces.Length != _nUsingNamespaces)
            {
                _usingNamespaces = new string[_nUsingNamespaces];
            }

            for (int i = 0; i < _nUsingNamespaces; i++)
            {
                _usingNamespaces[i] = EditorGUILayout.TextField(_usingNamespaces[i]);
            }

            EditorGUILayout.BeginHorizontal();

            _isChildClass = EditorGUILayout.Toggle("Derives From", _isChildClass);

            if (_isChildClass)
            {
                _derivesFrom = EditorGUILayout.TextField(_derivesFrom);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            _inCustomNamespace = EditorGUILayout.Toggle("Namespace", _inCustomNamespace);

            if (_inCustomNamespace)
            {
                _customNamespace = EditorGUILayout.TextField(_customNamespace);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (UnityEngine.GUILayout.Button("Preview"))
            {
                _scriptToGen = GenerateScript(
                    _scriptName,
                    _usingNamespaces,
                    _isChildClass ? _derivesFrom : string.Empty, 
                    _inCustomNamespace ? _customNamespace : string.Empty);
            }

            EditorGUILayout.TextArea(_scriptToGen);

            EditorGUILayout.Space();

            if (UnityEngine.GUILayout.Button("Generate"))
            {
                var path = EditorUtility.SaveFolderPanel("Save script to folder", "Assets", string.Empty);
                
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                
                SaveScript($"{path}/{_scriptName}.cs", _scriptName, _usingNamespaces, _derivesFrom, _customNamespace);
            }
        }

        
        private void SaveScript(string path, string scriptName, string[] usingNamespaces, string derivesFrom = "", string customNamespace = "")
        {
            var script = GenerateScript(scriptName, usingNamespaces, derivesFrom, customNamespace);
            System.IO.File.WriteAllText(path, script);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Saved script at {path}");
        }

        private string GenerateScript(string scriptName, string[] usingNamespaces, string derivesFrom = "", string customNamespace = "")
        {
            var inCustomNamespace = !string.IsNullOrEmpty(customNamespace);
            var res = string.Empty;

            for (int i = 0; i < usingNamespaces.Length; i++)
            {
                res += string.IsNullOrEmpty(usingNamespaces[i]) ? string.Empty : $"using {usingNamespaces[i]};\n";

                if (i == usingNamespaces.Length - 1)
                {
                    res += "\n";
                }
            }

            res += !inCustomNamespace ? string.Empty : $"namespace {customNamespace}\n{{\n";
            res += inCustomNamespace ? "\t" : string.Empty;
            res += $"public class {scriptName}";
            res += string.IsNullOrEmpty(derivesFrom) ? string.Empty : $" : {derivesFrom}";
            res += $"\n" + (inCustomNamespace ? "\t" : string.Empty) + "{\n" + (inCustomNamespace ? "\t" : string.Empty) + "}\n";
            res += !inCustomNamespace ? string.Empty : "}";

            return res;
        }
    }
}
