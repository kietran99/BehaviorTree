using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace RR.AI.BehaviorTree
{
    public class BTServiceSearchWindow : BTSearchWindowBase
    {
        protected override Func<Type, bool> ItemFilterPredicate => type => false;

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
