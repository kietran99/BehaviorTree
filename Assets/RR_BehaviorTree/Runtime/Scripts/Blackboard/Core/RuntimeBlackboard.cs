using UnityEngine;

using System.Collections.Generic;

namespace RR.AI
{
    public class RuntimeBlackboard
    {
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
            
            var valType = val.ValueType;

            if (valType != typeof(int)
                && valType != typeof(float)
                && valType != typeof(bool)
                && valType != typeof(string)
                && valType != typeof(Vector2)
                && valType != typeof(Vector3)
                && !typeof(UnityEngine.Object).IsAssignableFrom(valType))
            {
                Debug.LogError($"Unsupported type: {valType}");
                return false;
            }

            _map.Add(key, val);
            BBEventBroker.Instance.Publish(new BBAddEntryEvent<T>(key, val.value));
            return true;
        }

        public bool UpdateEntry<T>(string key, BBValue<T> val)
        {
            if (!_map.TryGetValue(key, out var oldVal))
			{
				Debug.LogWarning($"Key {key} does not exist");
				return false;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return false;
            }

			_map[key] = val;
            BBEventBroker.Instance.Publish(new BBUpdateEntryEvent<T>(key, (BBValue<T>)oldVal, val.value));
			return true;
        }

        public bool AddEntry(string key, int val) => AddEntry<int>(key, val);
        public bool AddEntry(string key, float val) => AddEntry<float>(key, val);
        public bool AddEntry(string key, bool val) => AddEntry<bool>(key, val);
        public bool AddEntry(string key, string val) => AddEntry<string>(key, val);
        public bool AddEntry(string key, Vector2 val) => AddEntry<Vector2>(key, val);
        public bool AddEntry(string key, Vector3 val) => AddEntry<Vector3>(key, val);
        public bool AddEntry(string key, UnityEngine.Object val) => AddEntry<UnityEngine.Object>(key, val);

        public bool UpdateEntry(string key, int val) => UpdateEntry<int>(key, val);
        public bool UpdateEntry(string key, float val) => UpdateEntry<float>(key, val);
        public bool UpdateEntry(string key, bool val) => UpdateEntry<bool>(key, val);
        public bool UpdateEntry(string key, string val) => UpdateEntry<string>(key, val);
        public bool UpdateEntry(string key, Vector2 val) => UpdateEntry<Vector2>(key, val);
        public bool UpdateEntry(string key, Vector3 val) => UpdateEntry<Vector3>(key, val);
        public bool UpdateEntry(string key, UnityEngine.Object val) => UpdateEntry<UnityEngine.Object>(key, val);

        public T GetValue<T>(string key) => ValueOr<T>(key, default);

        public T ValueOr<T>(string key, T defaultValue)
		{
			if (!_map.TryGetValue(key, out var val))
			{	
                Debug.LogWarning($"Key {key} was not found, returning default value instead");			
			    return defaultValue;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return defaultValue;
            }
     
            return (BBValue<T>)val;
		}
        
        public bool Remove(string key)
        {
            BBEventBroker.Instance.Publish(new BBDeleteEntryEvent(key));
            return _map.Remove(key);
        }
    
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
}
