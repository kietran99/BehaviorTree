using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BaseSubWindow : GraphElement
    {
        protected VisualElement Title(string title)
        {
            var container = new VisualElement();
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 5f;

            var label = new Label(title);
            label.style.fontSize = 14;
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;

            container.Add(label);

            return container;
        }

        protected VisualElement VerticalSpace()
        {
            var space = new VisualElement();
            space.style.height = EditorGUIUtility.singleLineHeight;
            return space;
        }
    }
}
