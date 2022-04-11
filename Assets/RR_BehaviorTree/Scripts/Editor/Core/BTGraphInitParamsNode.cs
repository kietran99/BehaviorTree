using UnityEngine;

using System;

namespace RR.AI.BehaviorTree
{
    public class BTGraphInitParamsNode
    {
        public Vector2 pos;
        public RR.AI.GraphBlackboard blackboard;
        public string name = "";
        public string desc = "";
        public string guid="";
        public Texture2D icon = null;
        public Action<Vector2, Action<Type>> OpenDecoSearchWindow;
    }
}