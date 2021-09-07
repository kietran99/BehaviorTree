using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI
{
	public class Blackboard : UnityEditor.Experimental.GraphView.Blackboard, IToolbarMenuElement
	{
        private System.Collections.Generic.Dictionary<string, IBlackboardItem> _itemDict; 

        public DropdownMenu menu { get; }

        public Blackboard(UnityEditor.Experimental.GraphView.GraphView graphView = null) : base(graphView)
		{
			_itemDict = new System.Collections.Generic.Dictionary<string, IBlackboardItem>();
			menu = CreateEntryMenu();
			addItemRequested = bb => OnAddItemRequested(bb);
		}

		private DropdownMenu CreateEntryMenu()
		{
			var menu = new DropdownMenu();

			menu.AppendAction("Int", _ => 
			{
				Add(CreateBBField("Integer", new IntegerField()));
			});

			menu.AppendAction("Bool", _ => 
			{
				Add(CreateBBField("Boolean", new Toggle()));
			});

			menu.AppendAction("Float", _ => 
			{
				Add(CreateBBField("Float", new FloatField()));
			});

			menu.AppendAction("String", _ => 
			{
				Add(CreateBBField("String", new TextField()));
			});

			menu.AppendAction("Vector2", _ => 
			{
				Add(CreateBBField("Vector2", new Vector2Field()));
			});

			menu.AppendAction("Vector3", _ => 
			{
				Add(CreateBBField("Vector3", new Vector3Field()));
			});

			menu.AppendAction("Transform", _ => 
			{
				Add(CreateBBField("Transform", new ObjectField() { objectType = typeof(Transform) }));
			});

			menu.AppendAction("Object", _ => 
			{
				Add(CreateBBField("Object", new ObjectField() { objectType = typeof(Object) }));
			});

			return menu;
		}

		private void OnAddItemRequested(UnityEditor.Experimental.GraphView.Blackboard blackboard) => this.ShowMenu();

		private VisualElement CreateBBField(string typeText, VisualElement propertyView)
		{
			var container = new VisualElement();
			var bbField = new BlackboardField() { text = "Key", typeText = typeText };
			container.Add(bbField);
			container.Add(new BlackboardRow(bbField, propertyView));
			return container;
		}

		public void OnGOSelectionChanged()
		{
			Clear();
		}

		public bool Add<T>(string key, T value)
		{
			if (_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict.Add(key, new BlackboardItem<T>(value));
			return true;
		}

		public bool Update<T>(string key, T value)
		{
			if (!_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict[key] = value as IBlackboardItem;
			return true;
		}

		public bool Remove(string key)
		{
			if (!_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict.Remove(key);
			return true;
		}
	}
}