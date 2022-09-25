using UnityEditor.Experimental.GraphView;

using System;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTServiceSearchWindow : BTSearchWindowBase
    {
        protected override Func<Type, bool> ItemFilterPredicate => type => 
                                                typeof(IBTService).IsAssignableFrom(type)
                                                && !type.IsAbstract
                                                && type != typeof(BTBaseTask)
                                                && !type.IsGenericType
                                                && type != typeof(BTTaskNull);

        protected override string BuiltInItemNamePrefix => "BTService";

        protected override List<SearchTreeEntry> CreateSearchTreeTopLevelEntries(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new UnityEngine.GUIContent("Add Service")),
            };
        }
    }
}
