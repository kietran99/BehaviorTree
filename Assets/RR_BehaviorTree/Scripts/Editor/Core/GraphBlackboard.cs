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

		private Queue<System.Func<GraphBlackboard, ScriptableObject, bool>> _cmdQueue;

        public GraphBlackboard(
			Blackboard runtimeBlackboard = null,
			ScriptableObject BBContainer = null,
			UnityEditor.Experimental.GraphView.GraphView graphView = null) : base(graphView)
		{
			if (BBContainer == null)
			{
				return;
			}

			_runtimeBB = runtimeBlackboard;
			_BBcontainer = BBContainer;
			_typeToKeysMap = runtimeBlackboard.TypeToKeysMap;
			_cmdQueue = new Queue<System.Func<GraphBlackboard, ScriptableObject, bool>>();

			addItemRequested = OnAddItemRequested;
			DisplayFields(runtimeBlackboard);

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
					_cmdQueue.Enqueue((blackboard, BBcontainer) => BBvalueInfo.AddToBlackboard(blackboard, key, valView, BBcontainer));
				}));
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
			var bbField = new BlackboardField() { text = string.IsNullOrEmpty(key) ? "Key" : key, typeText = typeText };
			container.Add(new BlackboardRow(bbField, propView));
			return container;
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

		public bool AddEntry<T>(string key, T value, ScriptableObject BBContainer, out IBBValue BBVal)
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

			var newEntryAsIBBValue = newEntry as IBBValue;
			BBVal = newEntry as IBBValue;

			return true;
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

		public bool RemoveEntry(string key, ScriptableObject BBContainer)
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

		public void WriteToDisk()
		{
			while (_cmdQueue.Count > 0)
			{
				var cmd = _cmdQueue.Dequeue();
				cmd(this, _BBcontainer);
			}
		}
	}
}