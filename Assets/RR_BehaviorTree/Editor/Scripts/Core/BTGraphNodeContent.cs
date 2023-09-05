using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeContent : VisualElement
    {
        private readonly Label _titleLabel;

        public string Title => _titleLabel.text;

        public BTGraphNodeContent(string title, Texture2D iconTex, string styleClassName)
        {
            styleSheets.Add(StylesheetUtils.Load("BTGraphNodeContent"));
            AddToClassList(styleClassName);
            _titleLabel = CreateTitleLabel(title);
            var icon = CreateIcon(iconTex);
            Add(icon);
            Add(_titleLabel);
        }

        public void Rename(string newName)
        {
            _titleLabel.text = newName;
        }

        private Label CreateTitleLabel(string title)
        {
            var titleLabel = new Label(title);
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.fontSize = 14;
            titleLabel.style.color = Color.white;
            return titleLabel;
        }

        private VisualElement CreateIcon(Texture2D iconTex)
        {
            var icon = new Image
            {
                image = iconTex,
                scaleMode = ScaleMode.ScaleToFit
            };
            icon.style.marginRight = 5;
            return icon;
        }
    }
}
