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
			editTextRequested += OnKeyEdited;
			
			_BBFieldToRowMap = DisplayFields(runtimeBlackboard);
			graphView.OnElementDeleted += OnElementDeleted;

			style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));
		}

		private void OnAddItemRequested(UnityEditor.Experimental.GraphView.Blackboard blackboard)
		{
			Add(new AddBBEntryBox(
				contentRect.width, 
				(key, valView, BBvalueInfo) => 
				{
					var addRes = BBvalueInfo.AddToBlackboard(this, key, valView, _BBcontainer, out var BBValue);

					if (!addRes)
					{
						return;
					}			

					var BBField = CreateBBField(BBvalueInfo.TypeText, key);
					var (BBEntry, fieldRowPair) = CreateBBEntry(BBValue.CreatePropView(), BBField);
					Add(BBEntry);
					_BBFieldToRowMap.Add(fieldRowPair.Key, fieldRowPair.Value);
				}));
		}

		private void OnKeyEdited(UnityEditor.Experimental.GraphView.Blackboard _, VisualElement BBField, string newKey)
		{
			if (_runtimeBB.TryGetValue(newKey, out var _))
			{
				Debug.LogError($"Key {newKey} already exists");
				return;
			}

			var convertedField = BBField as BlackboardField;
			var oldKey = convertedField.text;
			convertedField.text = newKey;
			UpdateKeyOnDisk(oldKey, newKey);
		}

		private Dictionary<GraphElement, VisualElement> DisplayFields(Blackboard runtimeBB)
		{
			if (runtimeBB == null)
			{
				return null;
			}

			var BBFieldToRowMap = new Dictionary<GraphElement, VisualElement>();

			var resultList = runtimeBB.Map<(VisualElement entry, KeyValuePair<GraphElement, VisualElement> BBfieldRowPair)>((key, val) => 
			{
				var BBVal = val as IBBValue;

				if (val == null)
				{
					Debug.LogError("Invalid cast from ScriptableObject to IBBValue");
					return (null, new KeyValuePair<GraphElement, VisualElement>(null, null));
				}

				var BBField = CreateBBField(BBVal.ValueTypeString, key);
				return CreateBBEntry(BBVal.CreatePropView(), BBField);
			});
			
			foreach(var result in resultList)
			{
				Add(result.entry);
				BBFieldToRowMap.Add(result.BBfieldRowPair.Key, result.BBfieldRowPair.Value);
			}

			return BBFieldToRowMap;
		}

		private (VisualElement entry, KeyValuePair<GraphElement, VisualElement> BBfieldRowPair) CreateBBEntry(
			VisualElement propView, BlackboardField blackboardField)
		{
			var container = new VisualElement();
			propView.style.width = contentRect.width - 16f;
			var entry = new BlackboardRow(blackboardField, propView);
			container.Add(entry);
			return (container, new KeyValuePair<GraphElement, VisualElement>(blackboardField, container));
		}

		private BlackboardField CreateBBField(string typeText, string key = "")
		{
			return new BlackboardField() { text = string.IsNullOrEmpty(key) ? "Key" : key, typeText = typeText };
		}

		private void OnElementDeleted(GraphElement element)
		{
			if (!_BBFieldToRowMap.TryGetValue(element, out var entry))
			{
				return;
			}

			_BBFieldToRowMap.Remove(element);
			Remove(entry);
			RemoveEntryOnDisk((element as BlackboardField).text, _BBcontainer);
		}

		public void OnGOSelectionChanged(Blackboard runtimeBlackboard = null, ScriptableObject BBContainer = null)
		{
			Clear();

			if (runtimeBlackboard == null || BBContainer == null)
			{
				return;
			}

			_runtimeBB = runtimeBlackboard;
			_BBcontainer = BBContainer;
			_typeToKeysMap = runtimeBlackboard.TypeToKeysMap;
			_BBFieldToRowMap = DisplayFields(runtimeBlackboard);
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