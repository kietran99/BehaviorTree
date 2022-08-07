using UnityEngine;

using System;
using System.Collections.Generic;

namespace RR.AI
{
    public class RuntimeBlackboard
    {
        private class BBRTValueGeneric<T> : IBBValueBase
        {
            private T _value;

            public BBRTValueGeneric(T value)
            {
                _value = value;
            }

            public static implicit operator T(BBRTValueGeneric<T> val) => val.Value;
            public static implicit operator BBRTValueGeneric<T>(T val) => new BBRTValueGeneric<T>(val);

            public object ValueAsObject => _value as object;
            public Type ValueType => typeof(T);

            public T Value { get => _value; set => _value = value; }

            public override string ToString() => _value.ToString();
        }

        private Dictionary<string, IBBValueBase> _map;

        public RuntimeBlackboard(Dictionary<string, IBBValueBase> map = null)
        {
            _map = map == null ? new Dictionary<string, IBBValueBase>() : map;
        }

        public bool AddEntry<T>(string key, BBValue<T> val)
        {
            if (_map.TryGetValue(key, out IBBValueBase _))
            {
                Debug.LogWarning($"Key {key} already exists");
                return false;
            }
            
            _map.Add(key, val);
            BBEventBroker.Instance.Publish(new BBAddEntryEvent<T>(key, val.value));
            return true;
        }

        public bool AddEntry(string key, int val) => AddEntry<int>(key, val);
        public bool AddEntry(string key, float val) => AddEntry<float>(key, val);
        public bool AddEntry(string key, bool val) => AddEntry<bool>(key, val);
        public bool AddEntry(string key, string val) => AddEntry<string>(key, val);
        public bool AddEntry(string key, Vector2 val) => AddEntry<Vector2>(key, val);
        public bool AddEntry(string key, Vector3 val) => AddEntry<Vector3>(key, val);
        public bool AddEntry(string key, UnityEngine.Object val) => AddEntry<UnityEngine.Object>(key, val);

        public T GetValue<T>(string key) => ValueOr<T>(key, default);

        public T ValueOr<T>(string key, T defaultValue)
		{
			if (!_map.TryGetValue(key, out var val))
			{	
                Debug.LogWarning($"No key {key} was found, returning default value");			
			    return defaultValue;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return defaultValue;
            }
     
            return val as BBRTValueGeneric<T>;
		}
    
        public bool Add<T>(string key, T value)
		{
			if (_map.TryGetValue(key, out var _))
			{
				Debug.LogWarning($"Key {key} already exists");
				return false;
			}

			_map.Add(key, new BBRTValueGeneric<T>(value));
			return true;
		}

        public bool Update<T>(string key, T value)
		{
			if (!_map.TryGetValue(key, out var val))
			{
				Debug.LogWarning($"Key {key} does not exist");
				return false;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return false;
            }

			(_map[key] as BBRTValueGeneric<T>).Value = value;
			return true;
		}
        
        public bool Remove(string key) => _map.Remove(key);
    
        public void Log()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{\n");

            foreach (var entry in _map)
            {
                sb.Append($"\t{entry.Key}: {entry.Value.ValueAsObject},\n");
            }

            sb.Append("}");

            Debug.Log(sb.ToString());
        }
    }

    public struct BBAddEntryEvent<T> : RR.Events.IEventData
    {
        public readonly string key;
        public readonly T value;

        public BBAddEntryEvent(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
