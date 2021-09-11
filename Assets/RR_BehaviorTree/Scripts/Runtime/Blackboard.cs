using UnityEngine;

namespace RR.AI
{
	public class Blackboard
	{
        private System.Collections.Generic.Dictionary<string, IBBEntry> _keyToItemMap;

        public Blackboard()
		{
			_keyToItemMap = new System.Collections.Generic.Dictionary<string, IBBEntry>();
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			if (_keyToItemMap.TryGetValue(key, out var val))
			{
				var convertedVal = val as BlackboardItem<T>;

				if (convertedVal == null)
				{
					Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {val.ValueType}");
				}

				value = convertedVal.Value;
				return true;
			}

			Debug.LogWarning($"{key} does not exist. Default {typeof(T)} value is returned instead.");
			value = default;
			return false;
		}

		public bool Add<T>(string key, T value)
		{
			if (_keyToItemMap.TryGetValue(key, out var _))
			{
				return false;
			}

			var newEntry = BBEntryFactory.New<T>(value);
			_keyToItemMap.Add(key, newEntry);

			return true;
		}

		public bool Update<T>(string key, T value)
		{
			if (!_keyToItemMap.TryGetValue(key, out var _))
			{
				return false;
			}

			_keyToItemMap[key] = value as IBBEntry;
			return true;
		}

		public bool Remove(string key)
		{
			if (!_keyToItemMap.TryGetValue(key, out var _))
			{
				return false;
			}

			_keyToItemMap.Remove(key);
			return true;
		}
	}
}