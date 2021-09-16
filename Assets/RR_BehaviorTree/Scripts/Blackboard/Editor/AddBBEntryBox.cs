using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace RR.AI
{
	public interface IBBValueInfo
	{
		string TypeText { get; }
		Func<VisualElement> CreateValView { get; set; }
		VisualElement CloneValView(VisualElement valView);
		bool AddToBlackboard(GraphBlackboard blackboard, string key, VisualElement valView, ScriptableObject BBContainer);
	}

	public class AddBBEntryBox : VisualElement
	{
        private class BBValueInfo<T> : IBBValueInfo
        {
            public BBValueInfo(string typeText, System.Func<VisualElement> createValView)
            {
                TypeText = typeText;
                CreateValView = createValView;
            }

            public string TypeText { get; }
            public Func<VisualElement> CreateValView { get; set; }

            public VisualElement CloneValView(VisualElement valView)
            {
                var converted = valView as BaseField<T>;

				if (converted == null)
				{
					Debug.LogError($"Invalid cast from VisualElement to BaseField<{typeof(T)}>");
					return new VisualElement();
				}

				var clone = CreateValView() as BaseField<T>;

				if (clone == null)
				{
					Debug.LogError($"Invalid cast from VisualElement to BaseField<{typeof(T)}>");
					return new VisualElement();
				}

				clone.value = converted.value;
				return clone;
            }

            public bool AddToBlackboard(GraphBlackboard blackboard, string key, VisualElement valView, ScriptableObject BBContainer)
            {
                var converted = valView as BaseField<T>;

				if (converted == null)
				{
					Debug.LogError($"Invalid cast from VisualElement to BaseField<{typeof(T)}>");
					return false;
				}

				return blackboard.AddEntry(key, converted.value, BBContainer, out var val);
            }
        }

        private Dictionary<string, IBBValueInfo> _menuBBTypeToValueInfomap;
		private List<string> _menuBBTypeOptions;

		private TextField _keyField;
		private VisualElement _valViewContainer, _curValView;
		private IBBValueInfo _curBBValueInfo;

		public AddBBEntryBox(
			float parentWidth, 
			Action<string, VisualElement, IBBValueInfo> addCallback = null, 
			Action cancelCallback = null)
		{
			_menuBBTypeToValueInfomap = new Dictionary<string, IBBValueInfo>()
			{
				{ "Int", new BBValueInfo<int>("Int", () => new IntegerField()) },
				{ "Bool", new BBValueInfo<bool>("Bool", () => new Toggle()) },
				{ "Float", new BBValueInfo<float>("Float", () => new FloatField()) },
				{ "String", new BBValueInfo<string>("String", () => new TextField()) },
				{ "Vector2", new BBValueInfo<Vector2>("Vector2", () => new Vector2Field()) },
				{ "Vector3", new BBValueInfo<Vector3>("Vector3", () => new Vector3Field()) },
				{ "Components/Transform", new BBValueInfo<Transform>("Transform", () => new ObjectField() { objectType = typeof(Transform) }) },
				{ "Object", new BBValueInfo<UnityEngine.Object>("Object", () => new ObjectField() { objectType = typeof(UnityEngine.Object) }) }
			};
			_menuBBTypeOptions = new List<string>(_menuBBTypeToValueInfomap.Keys);

			var BORDER_COLOR = new Color(26f / 255f, 26f / 255f, 26f / 255f);
			style.paddingBottom = 5f;
			style.paddingTop = 5f;
			style.backgroundColor = new Color(65f / 255f, 65f / 255f, 65f / 255f);
			style.borderTopWidth = 1f;
			style.borderTopColor = BORDER_COLOR;
			style.borderBottomWidth = 1f;
			style.borderBottomColor = BORDER_COLOR;

			var KEY_LABEL_WIDTH = 30f;
			var keyContainer = CreateKeyContainer(KEY_LABEL_WIDTH);
			_keyField = CreateKeyField(parentWidth - KEY_LABEL_WIDTH - 16f);
			keyContainer.Add(_keyField);
			Add(keyContainer);

			var DEFAULT_TYPE_TEXT = "Int";
			var TYPE_CHOICE_FIELD_WIDTH = 80f;
			_curBBValueInfo = _menuBBTypeToValueInfomap[DEFAULT_TYPE_TEXT];
			_valViewContainer = CreateValueContainer(TYPE_CHOICE_FIELD_WIDTH, DEFAULT_TYPE_TEXT, OnTypeChoiceChange);
			_curValView = CreateValueView(_curBBValueInfo, parentWidth - TYPE_CHOICE_FIELD_WIDTH);
			_valViewContainer.Add(_curValView);
			Add(_valViewContainer);

			Action onAddClick = null;
			onAddClick += () => parent.Remove(this);

			if (addCallback != null)
			{
				onAddClick += () => 
				{
					var valView = _curBBValueInfo.CloneValView(_curValView);
					addCallback(_keyField.value, valView, _curBBValueInfo);
				};
			}

			Action onCancelClick = null;
			onCancelClick += () => parent.Remove(this);

			if (cancelCallback != null)
			{
				onCancelClick += cancelCallback;
			}

			Add(CreateActionBtnContainer(onAddClick, onCancelClick));
		}

		private VisualElement CreateKeyContainer(float labelWidth)
		{
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			var keyLabel = new Label("Key");
			keyLabel.style.width = labelWidth;
			container.Add(keyLabel);	
			return container;
		}

		private TextField CreateKeyField(float width)
		{
			var keyField = new TextField();
			keyField.style.width = width;
			keyField.style.height = 16f;
			return keyField;
		}

		private VisualElement CreateValueContainer(
			float typeChoiceFieldWidth, 
			string defaultTypeText, 
			EventCallback<ChangeEvent<string>> typeChoiceChangeCallback)
		{
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			container.style.alignItems = Align.Center;
			
			var typeChoiceField = new PopupField<string>(_menuBBTypeOptions, defaultTypeText, 
				item => 
				{
					var res = item.Split('/');
					return res[res.Length - 1];
				}, null);
	
			typeChoiceField.style.width = typeChoiceFieldWidth;
			typeChoiceField.RegisterValueChangedCallback(typeChoiceChangeCallback);

			container.Add(typeChoiceField);

			return container;
		}

		private VisualElement CreateValueView(IBBValueInfo BBValueInfo, float width)
		{
			var curValView = BBValueInfo.CreateValView();
			curValView.style.width = width - 16f;
			curValView.style.height = 16f;
			return curValView;
		}

		private void OnTypeChoiceChange(ChangeEvent<string> evt)
		{
			_valViewContainer.Remove(_curValView);
			var (valViewWidth, valViewHeight) = (_curValView.style.width, _curValView.style.height);
			_curBBValueInfo = _menuBBTypeToValueInfomap[evt.newValue];
			_curValView = _curBBValueInfo.CreateValView();
			_curValView.style.width = valViewWidth;
			_curValView.style.height = valViewHeight;
			_valViewContainer.Add(_curValView);
		}

		private VisualElement CreateActionBtnContainer(Action addClickCallback, Action cancelClickCallback)
		{
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			container.style.justifyContent = Justify.Center;
			container.style.marginTop = 5f;
			container.style.paddingTop = 5f;
			container.style.borderTopWidth = 1f;
			container.style.borderTopColor = new Color(26f / 255f, 26f / 255f, 26f / 255f, .7f);

			var addButton = new Button(addClickCallback) { text = "Add" };
			var cancelButton = new Button(cancelClickCallback) { text = "Cancel" };
			container.Add(addButton);
			container.Add(cancelButton);

			return container;
		}
	}
}