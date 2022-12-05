using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTSubWndGraphDetails : BaseSubWindow
    {
        private BTDetailsPropFieldFactory _propFieldFactory;
        private TextField _nameField, _descField;
        private VisualElement _taskPropsContentContainer;
        private float _height, _width;

        public BTSubWndGraphDetails(UnityEngine.Rect rect)
        {
            _propFieldFactory = new BTDetailsPropFieldFactory();

            style.backgroundColor = RR.Utils.ColorExtension.Create(96f);
            SetPosition(rect);
            _height = rect.height;
            _width = rect.width;
            
            var titleContainer = Title("Details");
            Add(titleContainer);

            var generalContainer = CreateGeneralContainer();
            Add(generalContainer);

            var taskPropsContainer = CreateTaskPropsContainer();
            Add(taskPropsContainer);
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

        public void ShowNodeInfo(string name, string desc)
        {
            _nameField.value = name;
            _descField.value = desc;
        }

        public void ClearTaskPropsContent() => _taskPropsContentContainer.Clear();

        public void DrawTaskProp(BTTaskBase task)
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

                var UIField = new PropertyField(taskIter, taskIter.displayName);
                UIField.Bind(serializedTask);
                container.Add(UIField);
            }

            AddVerticalSpaceToFields(container);
            _taskPropsContentContainer.Add(container);
        }
    }
}
