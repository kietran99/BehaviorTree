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

            public Type ValueType => typeof(T);

            public T Value { get => _value; set => _value = value; }

            public override string ToString() => _value.ToString();
        }

        private System.Collections.Generic.Dictionary<string, IBBValueBase> _map;

        public RuntimeBlackboard(Dictionary<string, IBBValueBase> map = null)
        {
            _map = map == null ? new Dictionary<string, IBBValueBase>() : map;
        }

        public T GetValue<T>(string key) => ValueOr<T>(key, default);

        public T ValueOr<T>(string key, T defaultValue)
		{
			if (!_map.TryGetValue(key, out var val))
			{	
                UnityEngine.Debug.LogWarning($"No key {key} was found, returning default value");			
			    return defaultValue;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                UnityEngine.Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return defaultValue;
            }
     
            return val as BBRTValueGeneric<T>;
		}
    
        public bool Add<T>(string key, T value)
		{
			if (_map.TryGetValue(key, out var _))
			{
				UnityEngine.Debug.LogWarning($"Key {key} already exists");
				return false;
			}

			_map.Add(key, new BBRTValueGeneric<T>(value));
			return true;
		}

        public bool Update<T>(string key, T value)
		{
			if (!_map.TryGetValue(key, out var val))
			{
				UnityEngine.Debug.LogWarning($"Key {key} does not exist");
				return false;
			}

            if (!typeof(T).Equals(val.ValueType))
            {
                UnityEngine.Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
                return false;
            }

			(_map[key] as BBRTValueGeneric<T>).Value = value;
			return true;
		}
        
        public bool Remove(string key) => _map.Remove(key);
    
        public void Log()
        {
            foreach (var entry in _map)
            {
                UnityEngine.Debug.Log($"{entry.Key}: {entry.Value.ToString()}");
            }
        }
    }
}
