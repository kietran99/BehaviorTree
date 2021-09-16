namespace RR.Serialization
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : UnityEngine.ISerializationCallbackReceiver
    {
        private System.Collections.Generic.Dictionary<TKey, TValue> _map = new System.Collections.Generic.Dictionary<TKey, TValue>();

        [UnityEngine.SerializeField]
        private System.Collections.Generic.List<SerializableKeyValuePair<TKey, TValue>> _entries;

        public System.Collections.Generic.List<SerializableKeyValuePair<TKey, TValue>> Entries => _entries;

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            _map.Clear();
            
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_map.ContainsKey(_entries[i].Key))
                {
                    UnityEngine.Debug.LogWarning($"Duplicate key at index {i}, skipped");
                    continue;
                }

                _map.Add(_entries[i].Key, _entries[i].Value);
            }
        }
    
        public bool Add(TKey key, TValue value)
        {
            if (_map.ContainsKey(key))
            {
                return false;
            }

            if (_entries == null)
            {
                _entries = new System.Collections.Generic.List<SerializableKeyValuePair<TKey, TValue>>();
            }

            _entries.Add(new SerializableKeyValuePair<TKey, TValue>() { Key = key, Value = value });
            _map.Add(key, value);
            return true;
        }

        public bool Update(TKey key, TValue value)
        {
            if (!_map.ContainsKey(key))
            {
                return false;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Key.Equals(key))
                {
                    _entries[i].Value = value;
                    break;
                }
            }

            _map[key] = value;

            return true;
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            if (Add(key, value))
            {
                return true;
            }

            Update(key, value);

            return true;
        }

        public bool Remove(TKey key)
        {
            if (!_map.ContainsKey(key))
            {
                return false;
            }

            foreach (var entry in _entries)
            {
                if (entry.Key.Equals(key))
                {
                    _entries.Remove(entry);
                    OnAfterDeserialize();
                    return true;
                }
            }

            return false;
        }
            

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _map.TryGetValue(key, out value);
        }
	
		public System.Collections.Generic.IEnumerable<T> Map<T>(System.Func<TKey, TValue, T> fn)
		{
			foreach (var entry in _entries)
			{
				yield return fn(entry.Key, entry.Value);
			}
		}
    }

    [System.Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}
