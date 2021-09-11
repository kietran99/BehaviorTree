using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace RR.AI
{
	public class GraphBlackboard : UnityEditor.Experimental.GraphView.Blackboard, IToolbarMenuElement
	{
		private Blackboard _runtimeBB;
		private Dictionary<System.Type, List<string>> _typeToKeysMap;

		public Blackboard RuntimeBlackboard => _runtimeBB;

		public DropdownMenu menu { get; }

        public GraphBlackboard(UnityEditor.Experimental.GraphView.GraphView graphView = null) : base(graphView)
		{
			_runtimeBB = new Blackboard();
			_typeToKeysMap = new Dictionary<System.Type, List<string>>();
			style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));

			menu = CreateEntryMenu(new (string path, string typeName, VisualElement propView)[]
			{
				("Int", "Int", new IntegerField()),
				("Bool", "Bool", new Toggle()),
				("Float", "Float", new FloatField()),
				("String", "String", new TextField()),
				("Vector2", "Vector2", new Vector2Field()),
				("Vector3", "Vector3", new Vector3Field()),
				("Components/Transform", "Transform", new ObjectField() { objectType = typeof(Transform) }),
				("Object", "Object", new ObjectField() { objectType = typeof(Object) })
			});
			
			addItemRequested = bb => OnAddItemRequested(bb);
			AddEntry("Num Players", 4);
			AddEntry("Attacks Left", 21);
			// AddEntry("My Bool", true);
		}

		private DropdownMenu CreateEntryMenu((string path, string typeName, VisualElement propView)[] actionDataList)
		{
			var menu = new DropdownMenu();

			foreach (var data in actionDataList)
			{
				menu.AppendAction(data.path, _ => Add(CreateBBField(data.typeName, data.propView)));
			}
			return menu;
		}

		private void OnAddItemRequested(UnityEditor.Experimental.GraphView.Blackboard blackboard) => this.ShowMenu();

		private VisualElement CreateBBField(string typeText, VisualElement propView, string key = "")
		{
			var container = new VisualElement();
			var bbField = new BlackboardField() { text = string.IsNullOrEmpty(key) ? "Key" : key, typeText = typeText };
			container.Add(bbField);
			container.Add(new BlackboardRow(bbField, propView));
			return container;
		}

		public void OnGOSelectionChanged()
		{
			Clear();
		}

		public List<string> GetKeys(System.Type type)
		{
			if (_typeToKeysMap.TryGetValue(type, out var keys))
			{
				return keys;
			}

			return new List<string>();
		}

		public bool AddEntry<T>(string key, T value)
		{
			// if (_runtimeBB.TryGetValue(key, out T _))
			// {
			// 	return false;
			// }

			var newEntry = BBEntryFactory.New<T>(value);
			// _runtimeBB.Add(key, newEntry);

			var valType = typeof(T);

			if (!_typeToKeysMap.TryGetValue(valType, out var _))
			{
				_typeToKeysMap.Add(valType, new List<string>());
			}

			_typeToKeysMap[valType].Add(key);

			Add(CreateBBField(newEntry.ValueTypeString, newEntry.CreatePropField(), key));

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

		// public bool Remove(string key)
	}
}