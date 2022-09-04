using UnityEngine;
using UnityEditor.Experimental.GraphView;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    
    public abstract class BTSearchWindowBase : ScriptableObject, ISearchWindowProvider
    {
        private List<Type> _itemTypes;
        private Texture2D _indentation;

        public Action<Type, Vector2> OnEntrySelected { get; set; }

        public void Init()
        {
            _indentation = new Texture2D(1, 1);
            _indentation.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentation.Apply();
            
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _itemTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(ItemFilterPredicate);
                _itemTypes.AddRange(types);
            }
        }

        protected abstract Func<Type, bool> ItemFilterPredicate { get; }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {                  
            List<SearchTreeEntry> tree = CreateSearchTreeTopLevelEntries(context);

            foreach (Type type in _itemTypes)
            {    
                tree.Add(
                    new SearchTreeEntry(new GUIContent(GetItemName(type), _indentation))
                    {
                        userData = type,
                        level = 1
                    });
            }

            return tree;
        }

        protected abstract List<SearchTreeEntry> CreateSearchTreeTopLevelEntries(SearchWindowContext context);

        private string GetItemName(Type type)
        {
            string typeName = type.Name;
            string BTItemNamePrefix = BuiltInItemNamePrefix;
            string extractedTypeName = typeName.StartsWith(BTItemNamePrefix) ? typeName.Substring(BTItemNamePrefix.Length) : typeName;
            return RR.Utils.StringUtility.InsertWhiteSpaces(extractedTypeName);
        }

        protected abstract string BuiltInItemNamePrefix { get; }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {     
            OnEntrySelected?.Invoke(searchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}
