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

        public BTGraphOrderLabel(Vector2 pos, int order)
        {
            capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Droppable;

            var diameter = 16f;
            Rect rect = new Rect(pos.x, pos.y, diameter, diameter);
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
        }
    
        public void Move(Vector2 moveDelta)
        {
            var oldPos = GetPosition();
            SetPosition(new Rect(oldPos.x + moveDelta.x, oldPos.y + moveDelta.y, oldPos.width, oldPos.height));
        }

        public void SetRealPosition(Vector2 pos)
        {
            SetPosition(new Rect(pos, GetPosition().size));
        }
    }
}
