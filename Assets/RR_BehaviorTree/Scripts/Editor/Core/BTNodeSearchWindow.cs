using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private System.Collections.Generic.List<Type> _taskTypes = new System.Collections.Generic.List<Type>();
        private Action<Type, Vector2> OnEntrySelected;
        private Texture2D _indentation;

        public void Init(Action<Type, Vector2> entrySelectedCallback)
        {
            OnEntrySelected = entrySelectedCallback;

            _indentation = new Texture2D(1, 1);
            _indentation.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentation.Apply();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                                    .Where(type => typeof(BTBaseTask).IsAssignableFrom(type) 
                                                    && type != typeof(BTBaseTask)
                                                    && !type.IsGenericType
                                                    && type != typeof(BTTaskNull));

                _taskTypes.AddRange(types);
            }
        }

        public System.Collections.Generic.List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {                  
            var tree = new System.Collections.Generic.List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Add Node")),
                new SearchTreeGroupEntry(new GUIContent("Composite"), 1),
                new SearchTreeEntry(new GUIContent("Sequencer", _indentation))
                {
                    userData = typeof(BTGraphSequencer),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Selector", _indentation))
                {
                    userData = typeof(BTGraphSelector),
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Task"), 1)
            };

            foreach(var type in _taskTypes)
            {      
                tree.Add(
                    new SearchTreeEntry(new GUIContent(GetTaskName(type), _indentation))
                    {
                        userData = type,
                        level = 2
                    });
            }

            return tree;
        }

        private string GetTaskName(System.Type type)
        {
            var typeName = type.Name;
            var btTaskNamePrefix = "BTTask";
            var extractedTypeName = typeName.StartsWith(btTaskNamePrefix) ? typeName.Substring(btTaskNamePrefix.Length) : typeName;
            return RR.Utils.StringUtility.InsertWhiteSpaces(extractedTypeName);
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {     
            OnEntrySelected?.Invoke(searchTreeEntry.userData as System.Type, context.screenMousePosition);
            return true;
        }
    }
}