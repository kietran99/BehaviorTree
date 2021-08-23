using System;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseTask<TProp> : BTBaseTask where TProp : class, new()
    {
        [SerializeField]
        private BTTaskPropertyMap<TProp> _propMap;

        protected TProp Prop(string key) => LoadPropData(key) as TProp;

        public override Type PropertyType => typeof(TProp);
        
        public override bool SavePropData(string key, object data)
        {
            return _propMap.AddOrUpdate(key, data as TProp);
        }

        public override object LoadPropData(string key)
        {
            if (!_propMap.TryGetValue(key, out var data))
            {
                return new TProp();
            }

            return data;
        }
    }

    public abstract class BTBaseTask : ScriptableObject
    {
        [SerializeField]
        private Texture2D _icon = null;

        public abstract string Name { get; }
        public virtual System.Type PropertyType => typeof(BTTaskDataNone);
        public Texture2D Icon => _icon;

        public abstract void Init(GameObject actor, Blackboard blackboard, string nodeGuid);
        public abstract BTNodeState Tick(GameObject actor, Blackboard blackboard, string nodeGuid);
        public virtual bool SavePropData(string key, object data) => true;
        public virtual object LoadPropData(string key) 
        {
            return new BTTaskDataNone();
        }
    }
}
