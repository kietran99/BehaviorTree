using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public const string TASK_REF_CONTAINER_PATH = "BT_Tasks/BTRefsTask";

        private System.Collections.Generic.List<Type> taskTypes = new System.Collections.Generic.List<Type>();
        private Action<Node> OnEntrySelected;
        private Func<Vector2, Vector2> ContextToLocalMousePos;
        private Texture2D _indentation;

        public void Init(Action<Node> entrySelectCallback, Func<Vector2, Vector2> contextToLocalMousePos)
        {
            OnEntrySelected = entrySelectCallback;
            ContextToLocalMousePos = contextToLocalMousePos;

            _indentation = new Texture2D(1, 1);
            _indentation.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentation.Apply();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var taskReferences = Resources.Load<BTTaskReferenceContainer>(TASK_REF_CONTAINER_PATH);

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                                    .Where(type => typeof(BTBaseTask).IsAssignableFrom(type) && type != typeof(BTBaseTask));

                taskTypes.AddRange(types);

                // foreach (var type in types)
                // {
                //     Debug.Log($"{type} : {taskReferences.ExistsInstance(type)}");
                // }
            }

            // var instance = ScriptableObject.CreateInstance<BTTaskReferenceContainer>();
            // UnityEditor.AssetDatabase.CreateAsset(instance, $"Assets/Resources/BT_Tasks/BT_Task_Refs.asset");
            // UnityEditor.AssetDatabase.SaveAssets();
        }

        public System.Collections.Generic.List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {                  
            var tree = new System.Collections.Generic.List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Add Node")),
                new SearchTreeGroupEntry(new GUIContent("Composite"), 1),
                new SearchTreeEntry(new GUIContent("Sequencer", _indentation))
                {
                    userData = typeof(BTSequencer),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Selector", _indentation))
                {
                    userData = typeof(BTSelector),
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Task"), 1)
            };

            foreach(var type in taskTypes)
            {
                tree.Add(
                    new SearchTreeEntry(new GUIContent(type.Name, _indentation))
                    {
                        userData = typeof(BTLeaf<>).MakeGenericType(type),
                        level = 2
                    });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {     
            var nodeSpawnPos = ContextToLocalMousePos(context.screenMousePosition);
            var methodInfo = typeof(BTNodeFactory).GetMethod(nameof(BTNodeFactory.CreateNodeGeneric));
            var userDataType = searchTreeEntry.userData as System.Type;
            var genericMethodInfo = methodInfo.MakeGenericMethod(userDataType);
            var node = genericMethodInfo.Invoke(null, new object[] { nodeSpawnPos, string.Empty });
            OnEntrySelected?.Invoke(node as Node);
            return true;
        }
    }
}