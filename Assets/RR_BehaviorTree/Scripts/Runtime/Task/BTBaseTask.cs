using System;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public abstract class BTBaseTask<TProp> : BTBaseTask where TProp : class, new()
    {
        [SerializeField]
        private BTTaskPropertyMap<TProp> _propMap = null;

        protected override string DefaultIconPath => $"Icons/{GetType().Name}";

        public override Type PropertyType => typeof(TProp);
        
        public override void Init(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid)
        {
            Init(actor, blackboard, LoadPropData(nodeGuid) as TProp);
        }

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid)
        {
            return Tick(actor, blackboard, LoadPropData(nodeGuid) as TProp);
        }

        public abstract void Init(GameObject actor, RuntimeBlackboard blackboard, TProp prop);
        public abstract BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, TProp prop);

        #region SAVE & LOAD
        public override bool SavePropData(string key, object data)
        {
            if (_propMap == null)
            {
                _propMap = new BTTaskPropertyMap<TProp>();
            }

            var res = _propMap.AddOrUpdate(key, data as TProp);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            return res;
        }

        public override object LoadPropData(string key)
        {
            if (!_propMap.TryGetValue(key, out var data))
            {
                return new TProp();
            }

            return data;
        }

        public override bool RemoveProp(string key)
        {
            var res = _propMap.Remove(key);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            return res;
        }
        #endregion
    }

    public abstract class BTBaseTask : ScriptableObject
    {
        [SerializeField]
        private Texture2D _icon = null;

        public abstract string Name { get; }
        public virtual System.Type PropertyType => typeof(BTTaskDataNone);

        protected virtual string DefaultIconPath => string.Empty;
        public Texture2D Icon
        {
            get
            {
                if (_icon != null)
                {
                    return _icon;
                }
                       
                if (string.IsNullOrEmpty(DefaultIconPath))
                {
                    return null;
                }

                _icon = Resources.Load<Texture2D>(DefaultIconPath);
                return _icon;
            }
        }
            

        public abstract void Init(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid);
        public abstract BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid);
        public virtual bool SavePropData(string key, object data) => true;
        public virtual object LoadPropData(string key) 
        {
            return new BTTaskDataNone();
        } 
        public virtual bool RemoveProp(string key) => true;
    }
}
