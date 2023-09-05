using UnityEngine;
using UnityEngine.UIElements;

using System;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeAttacher : VisualElement, IBTIdentifiable
    {
        private const string STYLE_IDLE_UNSELECTED_BORDER = "idle-unselected-border";
        private const string STYLE_IDLE_SELECTED_BORDER = "idle-selected-border";
        private const string STYLE_HOVER_UNSELECTED_BORDER = "hover-unselected-border";
        private const string STYLE_HOVER_SELECTED_BORDER = "hover-selected-border";

        private readonly string _guid;
        private readonly string _nodeID;
        private readonly BTTaskBase _task;
        private readonly BTGraphNodeContent _content;

        private bool _selected;
        private string _curBorderStyle;

        public string Name { get; private set; }
        public string Guid => _guid;

        private static BTGraphNodeAttacher _curSelected = null;

        public Action<BTGraphNodeAttacher> MouseEntered { get; set; }
        public Action<BTGraphNodeAttacher> MouseExited { get; set; }

        public static BTGraphNodeAttacher CreateDecorator(BTGraphInitParamsAttacher initParams)
        {
            return new BTGraphNodeAttacher(initParams, "decorator");
        }

        public static BTGraphNodeAttacher CreateService(BTGraphInitParamsAttacher initParams)
        {
            return new BTGraphNodeAttacher(initParams, "service");
        }

        public BTGraphNodeAttacher(BTGraphInitParamsAttacher initParams, string styleClassName)
        {
            _content = new BTGraphNodeContent(initParams.name, initParams.icon, styleClassName);
            Add(_content);
            Name = initParams.name;
            _curBorderStyle = STYLE_IDLE_UNSELECTED_BORDER;
            _content.styleSheets.Add(StylesheetUtils.Load(nameof(BTGraphNodeAttacher)));
            _content.AddToClassList(_curBorderStyle);

            _selected = false;
            _content.RegisterCallback<MouseDownEvent>(OnSelected);
            _content.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _content.RegisterCallback<MouseLeaveEvent>(OnMouseExit);

            _nodeID = initParams.nodeID;
            _guid = initParams.guid;
            _task = initParams.task;
        }

        public void Rename(string name)
        {
            _content.Rename(name);
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
            BTGraphView.OnNewElementSelected?.Invoke(new ElementSelectParams()
                {
                    Guid = _guid,
                    Name = Name,
                    Desc = string.Empty,
                    Task = _task,
                    IsAttacher = true
                });
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
            MouseEntered?.Invoke(this);
        }
    
        private void OnMouseExit(MouseLeaveEvent evt)
        {
            SwapBorderStyle(_selected ? STYLE_IDLE_SELECTED_BORDER : STYLE_IDLE_UNSELECTED_BORDER);
            MouseExited?.Invoke(this);
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
            _content.RemoveFromClassList(_curBorderStyle);
            _content.AddToClassList(newStyle);
            _curBorderStyle = newStyle;
        }

        public void OnRemove()
        {
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(_task);
        }
    }
}
