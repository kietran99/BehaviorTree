using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using System.Collections.Generic;

namespace RR.AI
{
	public class GraphBlackboard : UnityEditor.Experimental.GraphView.Blackboard
	{
		private class RowContainer : VisualElement
		{
			public string key;
			public VisualElement propView;

            public RowContainer(string key, VisualElement propView, BlackboardRow row)
            {
                this.key = key;
                this.propView = propView;
				Add(row);
            }
        }

		private class CustomBlackboardField : BlackboardField
		{
			private System.Action<BlackboardField> _deleteCallback;

			public CustomBlackboardField(System.Action<BlackboardField> deleteCallback)
			{
				_deleteCallback = deleteCallback;
			}

			protected override void BuildFieldContextualMenu(ContextualMenuPopulateEvent evt)
			{
				base.BuildFieldContextualMenu(evt);
				evt.menu.InsertAction(0, "Delete", action => _deleteCallback?.Invoke(this));
			}
		}

        private Blackboard _blackboard;
		private ScriptableObject _BBcontainer;
		private Dictionary<System.Type, List<string>> _typeToKeysMap;
		private Dictionary<string, RowContainer> _keyToRowMap;
		private Dictionary<System.Type, string> _BBValueTypeNameMap;

        public GraphBlackboard(
			Blackboard serializableBB,
			ScriptableObject BBContainer,
			AbstractGraphView graphView) : base(graphView)
		{
			_blackboard = serializableBB;
			_BBcontainer = BBContainer;
			_typeToKeysMap = serializableBB.TypeToKeysMap;

			addItemRequested = OnAddItemRequested;
			editTextRequested += OnKeyEdited;
			
			_keyToRowMap = InitBBFields(serializableBB);

			_BBValueTypeNameMap = new Dictionary<System.Type, string>()
			{
				{ typeof(int), "Integer" },
				{ typeof(float), "Float" },
				{ typeof(bool), "Boolean" },
				{ typeof(string), "String" },
				{ typeof(Vector2), "Vector2" },
				{ typeof(Vector3), "Vector3" },
				{ typeof(Object), "Object" },
			};

			style.backgroundColor = new StyleColor(Utils.ColorExtension.Create(50f));
		}

		public bool AddEntry<T>(string key, BBValue<T> val)
		{
			return AddEntryInternal(key, val, _BBValueTypeNameMap[val.ValueType]);
		}

		private void OnAddItemRequested(UnityEditor.Experimental.GraphView.Blackboard blackboard)
		{
			var addBBEntryBox = new AddBBEntryBox(contentRect.width, OnAddItemConfirmed);
			this.Add(addBBEntryBox);
		}

		private void OnAddItemConfirmed(string key, VisualElement valView, IBBValueInfo BBvalueInfo)
		{
			var addRes = BBvalueInfo.AddToBlackboard(this, key, valView, _BBcontainer, out var val);

			if (!addRes)
			{
				return;
			}			

			AddEntryInternal(key, val, BBvalueInfo.TypeText);
		}

		private bool AddEntryInternal(string key, IBBValueBase val, string typeText)
		{
			BlackboardField BBField = CreateBBField(typeText, key);
			RowContainer rowContainer = CreateBBEntry(key, BBPropFieldFactory.Create(val), BBField);
			this.Add(rowContainer);
			_keyToRowMap.Add(key, rowContainer);
			return true;
		}

		private void OnKeyEdited(UnityEditor.Experimental.GraphView.Blackboard _, VisualElement BBField, string newKey)
		{
			if (_blackboard.TryGetValue(newKey, out var _))
			{
				Debug.LogError($"Key {newKey} already exists");
				return;
			}

			var convertedField = BBField as BlackboardField;
			var oldKey = convertedField.text;
			convertedField.text = newKey;
			UpdateKeyOnDisk(oldKey, newKey);
		}

		private Dictionary<string, RowContainer> InitBBFields(Blackboard blackboard)
		{
			if (blackboard == null)
			{
				return null;
			}

			var keyToRowMap = new Dictionary<string, RowContainer>();

			var resultList = blackboard.Map<(string key, RowContainer rowContainer)>((key, val) => 
			{
				var BBVal = val as IBBSerializableValue;

				if (val == null)
				{
					Debug.LogError("Invalid cast from ScriptableObject to IBBValue");
					return (string.Empty, new RowContainer(string.Empty, null, null));
				}

				BlackboardField BBField = CreateBBField(BBVal.ValueTypeString, key);
				VisualElement propField = BBPropFieldFactory.Create(BBVal);
				return (key, CreateBBEntry(key, propField, BBField));
			});
			
			foreach (var result in resultList)
			{
				Add(result.rowContainer);
				keyToRowMap.Add(result.key, result.rowContainer);
			}

			return keyToRowMap;
		}

		private RowContainer CreateBBEntry(string key, VisualElement propView, BlackboardField blackboardField)
		{
			propView.style.width = contentRect.width - 16f;
			var row = new BlackboardRow(blackboardField, propView);
			var rowContainer = new RowContainer(key, propView, row);
			return rowContainer;
		}

		private BlackboardField CreateBBField(string typeText, string key = "")
		{
			var field = new CustomBlackboardField(OnBBFieldDeleted) { text = string.IsNullOrEmpty(key) ? "Key" : key, typeText = typeText };
			return field;
		}

        private void OnBBFieldDeleted(BlackboardField field)
        {
			string key = field.text;
			RowContainer rowContainer = _keyToRowMap[key];
			_keyToRowMap.Remove(key);
			Remove(rowContainer);
			RemoveEntryOnDisk(key, _BBcontainer);
        }

		public void OnGOSelectionChanged(Blackboard serializableBB = null, ScriptableObject BBContainer = null)
		{
			Clear();

			if (serializableBB == null || BBContainer == null)
			{
				return;
			}

			_blackboard = serializableBB;
			_BBcontainer = BBContainer;
			_typeToKeysMap = serializableBB.TypeToKeysMap;
			_keyToRowMap = InitBBFields(serializableBB);
		}

		public List<string> GetKeys(System.Type type)
		{
			if (_typeToKeysMap.TryGetValue(type, out var keys))
			{
				return keys;
			}

			return new List<string>();
		}

		public bool AddEntryOnDisk<T>(string key, T value, ScriptableObject BBContainer, out IBBSerializableValue BBVal)
		{
			if (BBContainer == null)
			{
				Debug.LogError("Blackboard container Scriptable Object is Null");
				BBVal = null;
				return false;
			}

			if (_blackboard.TryGetValue(key, out T _))
			{
				BBVal = null;
				return false;
			}

			var newEntry = SerializableBBValueFactory.New<T>(BBContainer, value);
			
			if (newEntry == null)
			{
				BBVal = null;
				return false;
			}

			_blackboard.Add(key, newEntry);

			var valType = typeof(T);

			if (!_typeToKeysMap.TryGetValue(valType, out var _))
			{
				_typeToKeysMap.Add(valType, new List<string>());
			}

			_typeToKeysMap[valType].Add(key);

			BBVal = newEntry as IBBSerializableValue;

			return true;
		}

		public bool UpdateKeyOnDisk(string oldKey, string newKey) 
		{
			if (!_blackboard.TryGetValue(oldKey, out var valueSO))
			{
				return false;
			}

			var keys = _typeToKeysMap[(valueSO as IBBSerializableValue).ValueType];
			
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i] == oldKey)
				{
					keys[i] = newKey;
					break;
				}
			}

			return _blackboard.UpdateKey(oldKey, newKey);
		}

		public bool UpdateEntryOnDisk<T>(string key, BBValue<T> val)
		{
			return _blackboard.UpdateVal<T>(key, val);
		}

		public bool UpdateEntry<T>(string key, BBValue<T> val)
		{
			if (!_keyToRowMap.TryGetValue(key, out RowContainer rowContainer))
			{
				return false;
			}

			var castedPropView = rowContainer.propView as BaseField<T>;

			if (castedPropView == null)
			{
				Debug.LogWarning($"Type mismatch {typeof(T)}. Expecting type of {castedPropView.GetType()}");
				return false;
			}

			castedPropView.value = val;
			return true;
		}

		private bool RemoveEntryOnDisk(string key, ScriptableObject BBContainer)
		{
			if (BBContainer == null)
			{
				Debug.LogError("Blackboard container ScriptableObject is Null");
				return false;
			}

			if (!_blackboard.TryGetValue(key, out var valSO))
			{
				Debug.Log($"Invalid key {key}");
				return false;
			}

			_blackboard.Remove(key);
			UnityEditor.AssetDatabase.RemoveObjectFromAsset(valSO);
			UnityEditor.AssetDatabase.SaveAssets();

			return true;
		}
	
		public bool DeleteEntry(string key)
		{
			if (!_keyToRowMap.TryGetValue(key, out RowContainer row))
			{
				return false;
			}

			_keyToRowMap.Remove(key);
			Remove(row);
			return true;
		}
	}
}