using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeDecorator : VisualElement
    {
        private const string STYLE_IDLE_UNSELECTED_BORDER = "idle-unselected-border";
        private const string STYLE_IDLE_SELECTED_BORDER = "idle-selected-border";
        private const string STYLE_HOVER_UNSELECTED_BORDER = "hover-unselected-border";
        private const string STYLE_HOVER_SELECTED_BORDER = "hover-selected-border";

        private VisualElement _contentContainer;
        private bool _selected;
        private string _curBorderStyle;

        public string Name { get; private set; }
        
        public BTGraphNodeDecorator(string decoName, Texture2D icon)
        {
            _contentContainer = CreateTitleContent(decoName, icon);
            Name = decoName;
            _contentContainer.styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/BTNodeDecorator"));
            _curBorderStyle = STYLE_IDLE_UNSELECTED_BORDER;
            _contentContainer.AddToClassList(_curBorderStyle);
            
            Add(_contentContainer);

            _selected = false;
            RegisterCallback<MouseDownEvent>(OnSelected);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseExit);
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
            _selected = true;
            SwapBorderStyle(STYLE_HOVER_SELECTED_BORDER);
        }

        public void OnUnselected()
        {
            _selected = false;
            SwapBorderStyle(STYLE_IDLE_UNSELECTED_BORDER);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            SwapBorderStyle(_selected ? STYLE_HOVER_SELECTED_BORDER : STYLE_HOVER_UNSELECTED_BORDER);
        }
    
        private void OnMouseExit(MouseLeaveEvent evt)
        {
            SwapBorderStyle(_selected ? STYLE_IDLE_SELECTED_BORDER : STYLE_IDLE_UNSELECTED_BORDER);
        }

        private void SwapBorderStyle(string newStyle)
        {
            _contentContainer.RemoveFromClassList(_curBorderStyle);
            _contentContainer.AddToClassList(newStyle);
            _curBorderStyle = newStyle;
        }
    }
}
