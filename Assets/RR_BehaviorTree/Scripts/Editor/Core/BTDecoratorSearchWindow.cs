using UnityEditor.Experimental.GraphView;
using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTDecoratorSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private List<Type> _decoTypes;
        private Texture2D _indentation;
        private Action<Type> OnEntrySelected;

        public void Init()
        {
            _indentation = new Texture2D(1, 1);
            _indentation.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentation.Apply();
            
            _decoTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes()
                                            .Where(type => typeof(BTBaseTask).IsAssignableFrom(type) 
                                                && typeof(IBTBaseDecorator).IsAssignableFrom(type) 
                                                && type != typeof(BTBaseTask)
                                                && !type.IsGenericType
                                                && type != typeof(BTTaskNull));

                _decoTypes.AddRange(types);
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {                  
            var tree = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Add Decorator")),
            };

            foreach (var type in _decoTypes)
            {      
                tree.Add(
                    new SearchTreeEntry(new GUIContent(GetDecoName(type), _indentation))
                    {
                        userData = type,
                        level = 1
                    });
            }

            return tree;
        }

        private string GetDecoName(Type type)
        {
            var typeName = type.Name;
            var btDecoNamePrefix = "BTDeco";
            var extractedTypeName = typeName.StartsWith(btDecoNamePrefix) ? typeName.Substring(btDecoNamePrefix.Length) : typeName;
            return RR.Utils.StringUtility.InsertWhiteSpaces(extractedTypeName);
        }

        public void SetEntrySelectCallback(Action<Type> callback)
        {
            OnEntrySelected = callback;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {     
            OnEntrySelected?.Invoke(searchTreeEntry.userData as Type);
            return true;
        }
    }
}
