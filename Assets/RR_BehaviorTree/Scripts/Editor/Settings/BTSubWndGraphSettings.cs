using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

using System;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTSubWndGraphSettings : BaseSubWindow
    {
        private float _width, _height;
        private TextAsset _iconSettingsAsset;
        private BTSettingsNodeIconItem[] _iconSettingsList;
        private VisualElement _iconSettingsItemsContainer;

        private Action _onClose;
        private Action<int, string> _onIconSettingsChanged;

        public BTSubWndGraphSettings(TextAsset iconSettingsAsset, Action onCloseCallback, Rect rect)
        {
            capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Selectable;
            capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Ascendable;
            capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Movable;

            style.backgroundColor = new StyleColor(Utils.ColorExtension.Create(56f));

            _width = rect.width;
            _height = rect.height;
            SetPosition(rect);

            if (iconSettingsAsset != null)
            {
                _iconSettingsAsset = iconSettingsAsset;
                _onClose += onCloseCallback;

                Serialization
                    .JsonWrapper
                    .ReadJsonArray<BTSettingsNodeIconItem>(GetIconSettingsAssetPath(_iconSettingsAsset), out _iconSettingsList);
                
                Add(Title("Settings"));
                Add(IconSettingsAssetField(iconSettingsAsset));
                Add(IconSettingsSection(_iconSettingsList));
                Add(VerticalSpace());
                Add(OKButton());

                _onIconSettingsChanged = (idx, iconPath) => _iconSettingsList[idx].icon = iconPath;
            }   
        }

        public void Open()
        {
            SetEnabled(true);
        }

        private VisualElement IconSettingsAssetField(TextAsset nodeIconSettingsAsset)
        {
            var field = new ObjectField() 
            {
                value = nodeIconSettingsAsset,
                objectType = typeof(TextAsset),
                allowSceneObjects = false
            };

            field.style.marginBottom = 5f;
            field.SetEnabled(false);
            return field;
        }

        private VisualElement IconSettingsSection(BTSettingsNodeIconItem[] iconSettingsList)
        {
            var iconSettingsSection = new VisualElement();
            _iconSettingsItemsContainer = new VisualElement();
            VisualElement[] iconSettingsItems = IconSettingsItems(iconSettingsList);

            foreach (var item in iconSettingsItems)
            {
                _iconSettingsItemsContainer.Add(item);
            }

            iconSettingsSection.Add(_iconSettingsItemsContainer);
            iconSettingsSection.Add(IconSettingsActionBtnsContainer());
            return iconSettingsSection;
        }

        private VisualElement[] IconSettingsItems(BTSettingsNodeIconItem[] iconSettingsList)
        {
            var items = new VisualElement[iconSettingsList.Length];

            for (int i = 0; i < iconSettingsList.Length; i++)
            {
                var setting = iconSettingsList[i];

                var iconSettingsItem = new VisualElement();
                iconSettingsItem.style.flexDirection = FlexDirection.Row;
                // iconSettingsContainerItem.style.justifyContent = Justify.SpaceBetween;

                var taskNameField = new TextField() { value = setting.taskname };
                taskNameField.style.width = _width * .6f;
                taskNameField.SetEnabled(false);

                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(setting.icon);
                var iconAssetField = new ObjectField()
                {
                    value = icon,
                    objectType = typeof(Texture2D),
                    allowSceneObjects = false
                };

                iconAssetField.style.width = _width * .35f;
                var idx = i;
                iconAssetField.RegisterValueChangedCallback(evt => _onIconSettingsChanged(idx, AssetDatabase.GetAssetPath(evt.newValue)));

                iconSettingsItem.Add(taskNameField);
                iconSettingsItem.Add(iconAssetField);
                
                items[i] = iconSettingsItem;
            }

            return items;
        }

        private VisualElement IconSettingsActionBtnsContainer()
        {
            var iconSettingsActionContainer = new VisualElement();
            iconSettingsActionContainer.style.flexDirection = FlexDirection.Row;
            iconSettingsActionContainer.style.justifyContent = Justify.FlexEnd;
            iconSettingsActionContainer.style.paddingTop = 2f;

            var refreshBtn = ActionBtn(() => 
            {
                _iconSettingsList = RefreshIconSettings(_iconSettingsList);
                _iconSettingsItemsContainer.Clear();
                VisualElement[] iconSettingsItems = IconSettingsItems(_iconSettingsList);
                foreach (var item in iconSettingsItems)
                {
                    _iconSettingsItemsContainer.Add(item);
                }
            }, "Icons/General/Refresh");
            iconSettingsActionContainer.Add(refreshBtn);

            var saveBtn = ActionBtn(() => 
            {
                Serialization
                    .JsonWrapper
                    .OverwriteJsonArray<BTSettingsNodeIconItem>(GetIconSettingsAssetPath(_iconSettingsAsset), _iconSettingsList);

                UnityEditor.AssetDatabase.Refresh();
            }, "Icons/General/Save");
            iconSettingsActionContainer.Add(saveBtn);

            return iconSettingsActionContainer;
        }

        private Button ActionBtn(Action callback, string iconPath)
        {
            const float paddingTop = 2f;
            const float btnSize = 24f;
            const float iconSize = 16f;
            var btn = new Button(callback);
            var refreshIcon = new Image() { image = Resources.Load<Texture2D>(iconPath) };
            refreshIcon.style.width = iconSize;
            refreshIcon.style.height = iconSize;
            btn.Add(refreshIcon);
            btn.style.width = btnSize;
            btn.style.height = btnSize;
            btn.style.alignItems = Align.Center;
            btn.style.paddingTop = paddingTop;
            return btn;
        }

        private VisualElement OKButton()
        {
            var okBtn = new Button(() => 
            {
                _onClose?.Invoke();
                SetEnabled(false);
            }) { text = "OK" };

            return okBtn;
        }

        private BTSettingsNodeIconItem[] RefreshIconSettings(BTSettingsNodeIconItem[] oldSettingsList)
        {
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Func<BTSettingsNodeIconItem[], string, BTSettingsNodeIconItem> CreateSettings = (settings, taskName) =>
            {
                BTSettingsNodeIconItem setting = settings.FirstOrDefault(setting => setting.taskname == taskName);
                string icon = setting == null ? string.Empty : setting.icon;
                return new BTSettingsNodeIconItem(){ taskname = taskName, icon = icon };
            };

            var nodeIconSettingList = new System.Collections.Generic.List<BTSettingsNodeIconItem>(3)
            {
                CreateSettings(oldSettingsList, nameof(BTGraphRoot)),
                CreateSettings(oldSettingsList, nameof(BTGraphSelector)),
                CreateSettings(oldSettingsList, nameof(BTGraphSequencer)),
            };

            foreach (var assembly in assemblies)
            {

                var taskIconSettingList = assembly.GetTypes()
                                .Where(type => typeof(BTBaseTask).IsAssignableFrom(type) 
                                                && type != typeof(BTBaseTask)
                                                && !type.IsGenericType
                                                && type != typeof(BTTaskNull))
                                                .Select(taskType => CreateSettings(oldSettingsList, taskType.Name)); 

                nodeIconSettingList.AddRange(taskIconSettingList); 
            }

            return nodeIconSettingList.ToArray();
        }

        private string GetIconSettingsAssetPath(TextAsset iconSettings) => AssetDatabase.GetAssetPath(iconSettings);
    }
}
