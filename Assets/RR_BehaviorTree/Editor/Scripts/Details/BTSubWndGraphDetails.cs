using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public class BTSubWndGraphDetails : BaseSubWindow
    {
        private TextField _nameField, _descField;
        private VisualElement _taskPropsContentContainer;

        private EventCallback<ChangeEvent<string>> _curNameValueChangeCallback;

        public BTSubWndGraphDetails(UnityEngine.Rect rect)
        {
            style.backgroundColor = RR.Utils.ColorExtension.Create(96f);
            SetPosition(rect);
            
            var titleContainer = Title("Details");
            Add(titleContainer);

            var taskPropsContainer = CreateTaskPropsContainer();
            Add(taskPropsContainer);
            
            var generalContainer = CreateGeneralContainer();
            Add(generalContainer);
        }

        private VisualElement CreateContainerBase(string labelText, VisualElement content)
        {
            var container = new VisualElement();
            container.style.backgroundColor = RR.Utils.ColorExtension.Create(62f);

            var label = new Label(labelText);
            label.style.backgroundColor = RR.Utils.ColorExtension.Create(30f);
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            label.style.paddingLeft = 5f;
            label.style.paddingTop = 2f;
            label.style.paddingBottom = 2f;

            content.style.paddingTop = 2f;
            content.style.paddingBottom = 2f;

            AddVerticalSpaceToFields(content);

            container.Add(label);
            container.Add(content);

            return container;
        }

        private void AddVerticalSpaceToFields(VisualElement container)
        {
            foreach (var child in container.Children())
            {
                child.style.paddingBottom = 2f;
                child.style.paddingTop = 2f;
            }
        }

        private VisualElement CreateGeneralContainer()
        {
            var container = new VisualElement();
            _nameField = new TextField("Name");
            _nameField.labelElement.style.marginRight = -70;
            _descField = new TextField("Description") { multiline = true };
            _descField.labelElement.style.marginRight = -70;
            _descField.style.whiteSpace = WhiteSpace.Normal;

            container.Add(_nameField);
            container.Add(_descField);

            return CreateContainerBase("General", container);
        }

        private VisualElement CreateTaskPropsContainer()
        {
            var container = new VisualElement();
            _taskPropsContentContainer = new VisualElement();
            _taskPropsContentContainer.style.paddingLeft = 2f;
            container.Add(_taskPropsContentContainer);
            return CreateContainerBase("Properties", container);
        }

        public void ShowNodeInfo(SerializedProperty propName, System.Action<string> nameChangeCallback)
        {
            _nameField.BindProperty(propName);
            _nameField.UnregisterValueChangedCallback(_curNameValueChangeCallback);
            _curNameValueChangeCallback = evt => nameChangeCallback(evt.newValue);
            _nameField.RegisterValueChangedCallback(_curNameValueChangeCallback);
        }

        public void ClearTaskPropsContent() => _taskPropsContentContainer.Clear();

        public void DrawTaskProp(BTTaskBase task, GraphBlackboard blackboard)
        {
            ClearTaskPropsContent();
            var serializedTask = new SerializedObject(task);
            var container = new VisualElement();
            var taskIter = serializedTask.GetIterator();
            taskIter.NextVisible(true);

            while (taskIter.NextVisible(false))
            {
                string displayName = taskIter.displayName;

                if (displayName == "Script" || displayName == "Base")
                {
                    continue;
                }

                var taskType = taskIter.type;
                bool isBBKeySelector = taskType.StartsWith("BBKeySelector");
                var UIField = isBBKeySelector ? CreateBBKeySelectorField(taskIter, blackboard) : new PropertyField(taskIter, taskIter.displayName);
                UIField.Bind(serializedTask);
                container.Add(UIField);
            }

            AddVerticalSpaceToFields(container);
            _taskPropsContentContainer.Add(container);
        }

        private VisualElement CreateBBKeySelectorField(SerializedProperty task, GraphBlackboard blackboard)
        {
            var container = new VisualElement();
            SerializedPropertyType propType = task.FindPropertyRelative("typeVar").propertyType;
            var typeConversionMap = new Dictionary<SerializedPropertyType, System.Type>()
            {
                { SerializedPropertyType.ObjectReference, typeof(UnityEngine.Object) },
                { SerializedPropertyType.Boolean, typeof(bool) },
                { SerializedPropertyType.Vector2, typeof(UnityEngine.Vector2) }
            };
            
            System.Type valType = typeConversionMap[propType];
            List<string> BBKeys = blackboard.GetKeys(valType);

            if (BBKeys.Count == 0)
            {
                var label = new Label($"No value of type {valType}");
                label.style.whiteSpace = WhiteSpace.Normal;
                return label;
            }

            SerializedProperty propKey = task.FindPropertyRelative("_key");
            bool isInvalidFieldVal = string.IsNullOrEmpty(propKey.stringValue);
            string fieldValToAssign = isInvalidFieldVal ? BBKeys[0] : propKey.stringValue;
            var fieldKey = new PopupField<string>(task.displayName, BBKeys, fieldValToAssign);
            fieldKey.BindProperty(propKey);
            container.Add(fieldKey);
            return container;
        }
    }
}
