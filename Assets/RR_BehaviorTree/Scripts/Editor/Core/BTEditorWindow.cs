using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTEditorWindow : GraphViewEditorWindow
    {
        private BTGraphView _graphView;
        private BehaviorTree _inspectedBT;
        private BTNodeSearchWindow _searchWindow;
        private Toolbar _toolbar;

        public static System.Action OnClose { get; set; }

        public static void Init(BehaviorTree behaviorTree)
        {
            var window = GetWindow<BTEditorWindow>("Behavior Tree");
            window._inspectedBT = behaviorTree;
            window._graphView.Init(behaviorTree.DesignContainer);
        }

        private void CreateGUI()
        {
            _graphView = CreateGraphView();
            _searchWindow = CreateNodeSearchWindow(_graphView);
            _toolbar = CreateToolbar();

            rootVisualElement.Add(_graphView);
            rootVisualElement.Add(_toolbar);
        }

        private BTGraphView CreateGraphView()
        {
            var graphView = new BTGraphView();
            graphView.StretchToParentSize();
            return graphView;
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();          

            var saveBtn = new Button(() => 
            { 
                _inspectedBT.DesignContainer.Save(_graphView.nodes);
                _graphView.Save();
            }) { text = "Save Assets" };

            toolbar.Add(saveBtn);

            return toolbar;
        }

        private BTNodeSearchWindow CreateNodeSearchWindow(BTGraphView graphView)
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<BTNodeSearchWindow>();
            graphView.nodeCreationRequest += context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), window);
            System.Func<Vector2, Vector2> contextToLocalMousePos = contextMousePos =>
            {
                var worldPos = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, contextMousePos - position.position);
                return graphView.WorldToLocal(worldPos);
            };

            window.Init((nodeType, pos) => 
            {
                var localMousePos = contextToLocalMousePos(pos);
                var node = BTGraphNodeFactory.CreateDefaultGraphNode(
                    nodeType, 
                    _graphView.GetBlackboard() as GraphBlackboard, 
                    localMousePos,
                    graphView.DesignContainer.GetOrCreateTask);
                graphView.AddElement(node);
            });

            return window;
        }

        private void OnDisable()
        {
            OnClose?.Invoke();

            if (OnClose != null)
            {
                foreach (var listener in OnClose.GetInvocationList())
                {
                    OnClose -= (System.Action) listener;
                }
            }  

            rootVisualElement.Remove(_graphView);
            rootVisualElement.Remove(_toolbar);
        }
    }
}
