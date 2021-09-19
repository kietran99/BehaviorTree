using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace RR.AI
{
	public class GraphBlackboard : UnityEditor.Experimental.GraphView.Blackboard
	{
		private class BBEntry<T>
		{
			private string _key;
			private INotifyValueChanged<T> _valueView;
		}

		private Blackboard _runtimeBB;
		private ScriptableObject _BBcontainer;
		private Dictionary<System.Type, List<string>> _typeToKeysMap;

		private Dictionary<GraphElement, VisualElement> _BBFieldToRowMap;
		private Queue<System.Func<ScriptableObject, bool>> _cmdQueue;

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
			_cmdQueue = new Queue<System.Func<ScriptableObject, bool>>();

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
					_cmdQueue.Enqueue(BBcontainer => BBvalueInfo.AddToBlackboard(this, key, valView, BBcontainer));
				}));
		}

		private void OnEditKey(UnityEditor.Experimental.GraphView.Blackboard _, VisualElement BBField, string newText)
		{
			var convertedField = BBField as BlackboardField;
			var oldKey = convertedField.text;
			convertedField.text = newText;
			_cmdQueue.Enqueue(BBContainer => UpdateKeyOnDisk(oldKey, newText));
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
			_cmdQueue.Enqueue(BBcontainer => RemoveEntryOnDisk((element as BlackboardField).text, BBcontainer));
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

		private bool AddEntryOnDisk(string key, ScriptableObject valueContainer, ScriptableObject BBContainer)
		{
			var containerAsIBBValue = valueContainer as IBBValue;

			if (containerAsIBBValue == null)
			{
				Debug.LogError("Invalid cast from ScriptableObject to IBBValue");
				return false;
			}

			var valType = containerAsIBBValue.ValueType;

			if (!_typeToKeysMap.TryGetValue(valType, out var _))
			{
				_typeToKeysMap.Add(valType, new List<string>());
			}

			_runtimeBB.Add(key, valueContainer);
			_typeToKeysMap[valType].Add(key);
			return true;
		}

		public bool UpdateKeyOnDisk(string oldKey, string newKey) => _runtimeBB.Update(oldKey, newKey);

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
				Debug.LogError("Blackboard container Scriptable Object is Null");
				return false;
			}

			if (!_runtimeBB.TryGetValue(key, out var valSO))
			{
				return false;
			}

			_runtimeBB.Remove(key);
			UnityEditor.AssetDatabase.RemoveObjectFromAsset(valSO);
			UnityEditor.AssetDatabase.SaveAssets();

			return true;
		}

		public void SaveToDisk()
		{
			while (_cmdQueue.Count > 0)
			{
				var cmd = _cmdQueue.Dequeue();
				cmd(_BBcontainer);
			}
		}
	}
}