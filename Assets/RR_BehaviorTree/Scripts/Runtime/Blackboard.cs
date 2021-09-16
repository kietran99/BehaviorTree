using UnityEngine;

namespace RR.AI
{
	[System.Serializable]
	public class Blackboard
	{
		[SerializeField]
		private RR.Serialization.SerializableDictionary<string, ScriptableObject> _map;

		public bool TryGetValue<T>(string key, out T value)
		{
			if (_map.TryGetValue(key, out var SO))
			{
				if (SO is BBValue<T> valSO)
				{
					value = valSO.Value;
					return true;
				}

				Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {SO.GetType()}");
				value = default;
				return false;
			}

			value = default;
			return false;
		}

		public bool TryGetValue(string key, out ScriptableObject valueSO)
		{
			if (!_map.TryGetValue(key, out var SO))
			{
				valueSO = null;
				return false;
			}

			valueSO = SO;
			return true;
		}

		public bool Add<T>(string key, T value)
		{
			if (_map.TryGetValue(key, out var _))
			{
				Debug.LogWarning($"Key {key} not found");
				return false;
			}

			// var BBVal = BBValueFactory.New<T>(BBContainer, value);
			// _map.Add(key, BBVal);

			return true;
		}

		public bool Add(string key, ScriptableObject BBValue)
		{
			if (_map.TryGetValue(key, out var _))
			{
				Debug.LogWarning($"Key {key} not found");
				return false;
			}

			_map.Add(key, BBValue);

			return true;
		}

		public bool Update<T>(string key, T value)
		{
			// if (!_map.TryGetValue(key, out var SO))
			// {
			// 	Debug.LogWarning($"Key {key} not found");
			// 	return false;
			// }

			// if (!(SO is BBKeyValueDatabase<T> db))
			// {
			// 	Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {SO.GetType()}");
			// 	return false;
			// }

			// db.UpdateEntry(key, value);
			return true;
		}

		public bool Remove(string key) => _map.Remove(key);

		// public System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<string>> CreateTypeToKeysMap()
		// {
		// 	System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<string>> res = 
		// 		new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<string>>;

		// 	_map
		// }

		public System.Collections.Generic.IEnumerable<T> Map<T>(System.Func<string, ScriptableObject, T> fn) => _map.Map(fn);
	}
}