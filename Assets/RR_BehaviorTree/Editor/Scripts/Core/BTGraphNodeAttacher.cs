using UnityEngine;
using UnityEngine.UIElements;

using System;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeAttacher : VisualElement
    {
        private const string STYLE_IDLE_UNSELECTED_BORDER = "idle-unselected-border";
        private const string STYLE_IDLE_SELECTED_BORDER = "idle-selected-border";
        private const string STYLE_HOVER_UNSELECTED_BORDER = "hover-unselected-border";
        private const string STYLE_HOVER_SELECTED_BORDER = "hover-selected-border";

        private string _guid, _nodeID;
        private BTTaskBase _task;

        private VisualElement _contentContainer;
        private bool _selected;
        private string _curBorderStyle;

        public string Name { get; private set; }

        private static BTGraphNodeAttacher _curSelected = null;

        public Action MouseEntered { get; set; }
        public Action MouseExited { get; set; }

        public static BTGraphNodeAttacher CreateDecorator(BTGraphInitParamsAttacher initParams)
        {
            return new BTGraphNodeAttacher(initParams, "BTNodeDecorator");
        }

        public static BTGraphNodeAttacher CreateService(BTGraphInitParamsAttacher initParams)
        {
            return new BTGraphNodeAttacher(initParams, "BTNodeService");
        }

        public BTGraphNodeAttacher(BTGraphInitParamsAttacher initParams, string styleSheetName)
        {
            _contentContainer = CreateTitleContent(initParams.name, initParams.icon);
            Name = initParams.name;
            _contentContainer.styleSheets.Add(Resources.Load<StyleSheet>($"Stylesheets/{styleSheetName}"));
            _curBorderStyle = STYLE_IDLE_UNSELECTED_BORDER;
            _contentContainer.AddToClassList(_curBorderStyle);
            
            Add(_contentContainer);

            _selected = false;
            RegisterCallback<MouseDownEvent>(OnSelected);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseExit);

            _nodeID = initParams.nodeID;
            _guid = initParams.guid;
            _task = initParams.task;
        }

        private VisualElement CreateTitleContent(string title, Texture2D nodeIcon)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            
            var icon = new Image();
            icon.image = nodeIcon;
            icon.scaleMode = ScaleMode.ScaleToFit;
            icon.style.marginRight = 5;
            container.Add(icon);

            var titleLabel = new Label(title);
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.fontSize = 14;
            titleLabel.style.color = Color.white;
            container.Add(titleLabel);

            return container;
        }

        private void OnSelected(MouseDownEvent evt)
        {
            if (_curSelected != null)
            {
                _curSelected.OnUnselected();
            }

            _curSelected = this;
            _selected = true;
            SwapBorderStyle(STYLE_HOVER_SELECTED_BORDER);
            BTGraphView.OnNewNodeSelected?.Invoke(_guid, Name, string.Empty, _task);
            evt.StopPropagation();
        }

        public void OnUnselected()
        {
            _selected = false;
            SwapBorderStyle(STYLE_IDLE_UNSELECTED_BORDER);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            SwapBorderStyle(_selected ? STYLE_HOVER_SELECTED_BORDER : STYLE_HOVER_UNSELECTED_BORDER);
            MouseEntered?.Invoke();
        }
    
        private void OnMouseExit(MouseLeaveEvent evt)
        {
            SwapBorderStyle(_selected ? STYLE_IDLE_SELECTED_BORDER : STYLE_IDLE_UNSELECTED_BORDER);
            MouseExited?.Invoke();
        }

        public static void OnNodeUnselected(string nodeID)
        {
            if (_curSelected == null || nodeID != _curSelected._nodeID)
            {
                return;
            }

            _curSelected.OnUnselected();
            _curSelected = null;
        }

        private void SwapBorderStyle(string newStyle)
        {
            _contentContainer.RemoveFromClassList(_curBorderStyle);
            _contentContainer.AddToClassList(newStyle);
            _curBorderStyle = newStyle;
        }
    }
}
