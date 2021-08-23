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
        private Blackboard _blackboard;
        private Toolbar _toolbar;

        [MenuItem("Graph/Behavior Tree")]
        public static void Init()
        {
            var window = GetWindow<BTEditorWindow>("Behavior Tree");
            // window.position = new Rect(500, 500, 500, 500);
            // window.minSize = new Vector2(800, 800);
        }

        private void CreateGUI()
        {
            (_graphView, _inspectedBT) = CreateGraphView();

            _searchWindow = CreateNodeSearchWindow(_graphView);

            _blackboard = CreateBlackboard(_graphView, "Shared Variables", new Rect(10, 30, 250, 250));
            _blackboard.visible = _inspectedBT != null;

            _toolbar = CreateToolbar();
            _toolbar.visible = _inspectedBT != null;

            Selection.selectionChanged += OnGOSelectionChanged;
            
            _graphView.Add(_blackboard);

            rootVisualElement.Add(_graphView);
            rootVisualElement.Add(_toolbar);
        }

        private (BTGraphView, BehaviorTree) CreateGraphView()
        {
            if (Selection.activeGameObject == null)
            {
                var emptyGraphView = new BTGraphView();
                emptyGraphView.StretchToParentSize();  
                return (emptyGraphView, null);
            }

            var res = TryGetBehaviorTree(Selection.activeGameObject, out var BT);
            var graphView = res ? new BTGraphView(BT.DesignContainer) : new BTGraphView();
            graphView.StretchToParentSize();
            return (graphView, res ? BT : null);
        }

        private Blackboard CreateBlackboard(BTGraphView graphView, string title, Rect rect)
        {
            var blackboard = new Blackboard(graphView) { title = title, scrollable = true };
            blackboard.SetPosition(rect);
            blackboard.addItemRequested = bb => AddBlackboardItem(bb);
            return blackboard;
        }

        private void AddBlackboardItem(UnityEditor.Experimental.GraphView.Blackboard blackboard)
        {
            var container = new VisualElement();
            var bbField = new BlackboardField() { text = "New Key", typeText = "Some Type" };
            container.Add(bbField);
            var propertyView = new ObjectField() { objectType = typeof(Object) };
            container.Add(new BlackboardRow(bbField, propertyView));
            blackboard.Add(container);
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();          

            var saveBtn = new Button(() => 
            { 
                if (_inspectedBT != null) 
                {
                    _inspectedBT.DesignContainer.Save(_graphView.nodes);
                }
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

            window.Init(node => graphView.AddElement(node), contextToLocalMousePos);
            return window;
        }

        private void OnGOSelectionChanged()
        {
            var selectedGO = Selection.activeGameObject;

            if (selectedGO == null)
            {
                return;
            }

            if (!TryGetBehaviorTree(selectedGO, out _inspectedBT))
            {
                _graphView.OnGOSelectionChanged(null);
                _toolbar.visible = false;
                _blackboard.visible = false;
                return;
            }

            _toolbar.visible = true;
            _blackboard.visible = true;
            _graphView.OnGOSelectionChanged(_inspectedBT.DesignContainer);
        }

        private bool TryGetBehaviorTree(GameObject GO, out BehaviorTree behaviorTree)
        {
            if (!GO.TryGetComponent<BehaviorTree>(out var BT))
            {
                behaviorTree = null;
                return false;
            }

            behaviorTree = BT;
            return true;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnGOSelectionChanged;
            // _graphView.RemoveElement(_blackboard);
            rootVisualElement.Remove(_graphView);
            rootVisualElement.Remove(_toolbar);
        }
    }
}
