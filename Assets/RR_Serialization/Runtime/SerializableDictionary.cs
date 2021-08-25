namespace RR.Serialization
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : UnityEngine.ISerializationCallbackReceiver
    {
        private System.Collections.Generic.Dictionary<TKey, TValue> _map = new System.Collections.Generic.Dictionary<TKey, TValue>();

        public System.Collections.Generic.List<SerializableKeyValuePair<TKey, TValue>> Entries;

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            _map.Clear();
            
            for (int i = 0; i < Entries.Count; i++)
            {
                if (_map.ContainsKey(Entries[i].Key))
                {
                    UnityEngine.Debug.LogWarning($"Duplicate key at index {i}, skipped");
                    continue;
                }

                _map.Add(Entries[i].Key, Entries[i].Value);
            }
        }
    
        public bool Add(TKey key, TValue value)
        {
            if (_map.ContainsKey(key))
            {
                return false;
            }

            if (Entries == null)
            {
                Entries = new System.Collections.Generic.List<SerializableKeyValuePair<TKey, TValue>>();
            }

            Entries.Add(new SerializableKeyValuePair<TKey, TValue>() { Key = key, Value = value });
            _map.Add(key, value);
            return true;
        }

        public bool Update(TKey key, TValue value)
        {
            if (!_map.ContainsKey(key))
            {
                return false;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Key.Equals(key))
                {
                    Entries[i].Value = value;
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

        public bool Remove(TKey key) => _map.Remove(key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _map.TryGetValue(key, out value);
        }
    }

    [System.Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}
