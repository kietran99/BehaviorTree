using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace RR.AI
{
	public class GraphBlackboard : UnityEditor.Experimental.GraphView.Blackboard
	{
		private Blackboard _runtimeBB;
		private ScriptableObject _BBcontainer;
		private Dictionary<System.Type, List<string>> _typeToKeysMap;

		private Dictionary<GraphElement, VisualElement> _BBFieldToRowMap;

        public GraphBlackboard(
			Blackboard runtimeBlackboard = null,
			ScriptableObject BBContainer = null,
			AbstractGraphView graphView = null) : base(graphView)
		{
			if (BBContainer == null)
			{
				return;
			}

			_runtimeBB = runtimeBlackboard;
			_BBcontainer = BBContainer;
			_typeToKeysMap = runtimeBlackboard.TypeToKeysMap;

			addItemRequested = OnAddItemRequested;
			editTextRequested += OnEditKey;
			
			_BBFieldToRowMap = new Dictionary<GraphElement, VisualElement>();
			DisplayFields(runtimeBlackboard);
			graphView.OnElementDeleted += OnElementDeleted;

			style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));
		}

		private void OnAddItemRequested(UnityEditor.Experimental.GraphView.Blackboard blackboard)
		{
			Add(new AddBBEntryBox(
				contentRect.width, 
				(key, valView, BBvalueInfo) => 
				{
					var BBField = CreateBBField(BBvalueInfo.TypeText, valView, key);
					Add(BBField);
					BBvalueInfo.AddToBlackboard(this, key, valView, _BBcontainer);
				}));
		}

		private void OnEditKey(UnityEditor.Experimental.GraphView.Blackboard _, VisualElement BBField, string newKey)
		{
			var convertedField = BBField as BlackboardField;
			var oldKey = convertedField.text;
			convertedField.text = newKey;
			UpdateKeyOnDisk(oldKey, newKey);
		}

		private void DisplayFields(Blackboard runtimeBB)
		{
			if (runtimeBB == null)
			{
				return;
			}

			var fields = runtimeBB.Map((key, val) => 
			{
				var BBVal = val as IBBValue;

				if (val == null)
				{
					Debug.LogError("Invalid cast from ScriptableObject to IBBValue");
					return new VisualElement();
				}

				return CreateBBField(BBVal.ValueTypeString, BBVal.CreatePropField(), key);
			});
			
			foreach(var field in fields)
			{
				Add(field);
			}
		}

		private VisualElement CreateBBField(string typeText, VisualElement propView, string key = "")
		{
			var container = new VisualElement();
			propView.style.height = 16f;
			propView.style.width = contentRect.width - 16f;
			var BBField = new BlackboardField() { text = string.IsNullOrEmpty(key) ? "Key" : key, typeText = typeText };
			var entry = new BlackboardRow(BBField, propView);
			container.Add(entry);
			_BBFieldToRowMap.Add(BBField, container);
			return container;
		}

		private void OnElementDeleted(GraphElement element)
		{
			if (!_BBFieldToRowMap.TryGetValue(element, out var entry))
			{
				return;
			}

			Remove(entry);
			RemoveEntryOnDisk((element as BlackboardField).text, _BBcontainer);
		}

		public void OnGOSelectionChanged(Blackboard runtimeBlackboard = null, ScriptableObject BBContainer = null)
		{
			Clear();

			if (runtimeBlackboard != null && BBContainer != null)
			{
				DisplayFields(runtimeBlackboard);
				_BBcontainer = BBContainer;
			}
		}

		public List<string> GetKeys(System.Type type)
		{
			if (_typeToKeysMap.TryGetValue(type, out var keys))
			{
				return keys;
			}

			return new List<string>();
		}

		public bool AddEntryOnDisk<T>(string key, T value, ScriptableObject BBContainer, out IBBValue BBVal)
		{
			if (BBContainer == null)
			{
				Debug.LogError("Blackboard container Scriptable Object is Null");
				BBVal = null;
				return false;
			}

			if (_runtimeBB.TryGetValue(key, out T _))
			{
				BBVal = null;
				return false;
			}

			var newEntry = BBValueFactory.New<T>(BBContainer, value);

			if (newEntry == null)
			{
				BBVal = null;
				return false;
			}

			_runtimeBB.Add(key, newEntry);

			var valType = typeof(T);

			if (!_typeToKeysMap.TryGetValue(valType, out var _))
			{
				_typeToKeysMap.Add(valType, new List<string>());
			}

			_typeToKeysMap[valType].Add(key);

			BBVal = newEntry as IBBValue;

			return true;
		}

		public bool UpdateKeyOnDisk(string oldKey, string newKey) 
		{
			if (!_runtimeBB.TryGetValue(oldKey, out var valueSO))
			{
				return false;
			}

			var keys = _typeToKeysMap[(valueSO as IBBValue).ValueType];
			
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i] == oldKey)
				{
					keys[i] = newKey;
					break;
				}
			}

			return _runtimeBB.Update(oldKey, newKey);
		}

		// public bool Update<T>(string key, T value)
		// {
		// 	if (!_runtimeBlackboard.TryGetValue(key, out var _))
		// 	{
		// 		return false;
		// 	}

		// 	_keyToItemMap[key] = value as IBBEntry;
		// 	return true;
		// }

		private bool RemoveEntryOnDisk(string key, ScriptableObject BBContainer)
		{
			if (BBContainer == null)
			{
				Debug.LogError("Blackboard container ScriptableObject is Null");
				return false;
			}

			if (!_runtimeBB.TryGetValue(key, out var valSO))
			{
				Debug.Log($"Invalid key {key}");
				return false;
			}

			_runtimeBB.Remove(key);
			UnityEditor.AssetDatabase.RemoveObjectFromAsset(valSO);
			UnityEditor.AssetDatabase.SaveAssets();

			return true;
		}
	}
}