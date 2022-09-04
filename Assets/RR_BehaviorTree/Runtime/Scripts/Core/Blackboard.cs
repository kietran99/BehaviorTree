using UnityEngine;

using System.Collections.Generic;

using RR.Serialization;

namespace RR.AI
{
	[CreateAssetMenu(fileName = "Blackboard", menuName = "Generator/AI/Blackboard")]
	public class Blackboard : ScriptableObject
	{
		[SerializeField]
		private SerializableDictionary<string, ScriptableObject> _map = new SerializableDictionary<string, ScriptableObject>();

		public bool TryGetValue<T>(string key, out T value)
		{
			if (_map.TryGetValue(key, out var SO))
			{
				if (SO is BBSerializableValue<T> valSO)
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

		public T ValueOr<T>(string key, T defaultValue)
		{
			if (_map.TryGetValue(key, out var SO))
			{
				if (SO is BBSerializableValue<T> valSO)
				{
					return valSO.Value;
				}

				Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {SO.GetType()}");
				return defaultValue;
			}

			return defaultValue;
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

		public bool UpdateKey(string oldKey, string newKey) => _map.Update(oldKey, newKey);

		public bool UpdateVal<T>(string key, T value)
		{
			if (!_map.TryGetValue(key, out ScriptableObject BBValContainer))
			{
				Debug.LogWarning($"Key {key} not found");
				return false;
			}

			var containerAsBBValue = BBValContainer as IBBSerializableValue;

			if (typeof(T) != containerAsBBValue.ValueType)
			{
				Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {containerAsBBValue.ValueType}");
				return false;
			}

			containerAsBBValue.SetValue(value);
			return true;
		}

		public bool Remove(string key) => _map.Remove(key);

		public Dictionary<System.Type, List<string>> TypeToKeysMap
		{
			get
			{
				Dictionary<System.Type, List<string>> res = new Dictionary<System.Type, List<string>>();

				var entries = _map.Entries;

				if (entries == null)
				{
					return null;
				}

				foreach (var entry in entries)
				{
					var value = entry.Value as IBBSerializableValue;

					if (value == null)
					{
						continue;
					}

					var type = value.ValueType;

					if (!res.TryGetValue(type, out var _))
					{
						res.Add(type, new List<string>());
					}

					res[type].Add(entry.Key);
				}
				
				return res;
			}
		}

		public IEnumerable<T> Map<T>(System.Func<string, ScriptableObject, T> fn) => _map.Map(fn);

		public RuntimeBlackboard CreateRuntimeBlackboard()
		{
			var runtimeBB = new RuntimeBlackboard();

			foreach (var entry in _map.Entries)
			{
				(entry.Value as IBBSerializableValue).AddToRuntimeBlackboard(runtimeBB, entry.Key);
			}

			return runtimeBB;
		}
	}
}