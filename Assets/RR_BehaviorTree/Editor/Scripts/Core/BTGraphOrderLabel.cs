using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphOrderLabel : GraphElement
    {
        private Label _txtLb;

        public int Value 
        {
            get => int.Parse(_txtLb.text);
            set => _txtLb.text = value.ToString();
        }

        public BTGraphOrderLabel(IInteractable attachee, int order)
        {
            capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Droppable;

            var diameter = 16f;
            var rect = new Rect(0.0f, 0.0f, diameter, diameter);
            SetPosition(rect);

            style.backgroundColor = Utils.ColorExtension.Create(145f);
            var radius = 90;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.justifyContent = Justify.Center;

            // styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/BTGraphOrderLabel"));

            string orderTxt = order.ToString();
            _txtLb = new Label(orderTxt);
            _txtLb.style.width = diameter;
            _txtLb.style.height = diameter;

            if (orderTxt.Length > 1)
            {
                style.alignItems = Align.Center;
                _txtLb.style.alignItems = Align.Center;
                _txtLb.style.justifyContent = Justify.Center;
            }

            _txtLb.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(_txtLb);

            attachee.MoveStarted += OnAttacheeMoveStarted;
            attachee.MoveEnded += OnAttacheeMoveEnded;
            attachee.Selected += OnAttacheeSelected;
        }

        private void OnAttacheeMoveStarted()
        {
            SetEnabled(true);
            visible = true;
        }

        private void OnAttacheeMoveEnded()
        {
            visible = false;
            SetEnabled(false);
        }

        private void OnAttacheeSelected()
        {
            BringToFront();
        }
    }
}
