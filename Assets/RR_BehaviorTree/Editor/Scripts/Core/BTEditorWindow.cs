using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTEditorWindow : GraphViewEditorWindow
    {
        [SerializeField]
        private GameObject _BTGameObj = null;
        private BehaviorTree _inspectedBT;
        private BTGraphView _graphView;
        private BTNodeSearchWindow _searchWindow;
        private Toolbar _toolbar;

        public static System.Action OnClose { get; set; }

        public static void Init(BehaviorTree behaviorTree)
        {
            var window = GetWindow<BTEditorWindow>("Behavior Tree");
            window._BTGameObj = behaviorTree.gameObject;
            window.CreateGUI();
        }

        private void CreateGUI()
        {
            if (_BTGameObj == null)
            {
                return;
            }
            
            if (!_BTGameObj.TryGetComponent<BehaviorTree>(out _inspectedBT))
            {
                Debug.LogError($"No BehaviorTree script attached on GameObject {_BTGameObj.name}");
                return;
            }

            _graphView = CreateGraphView(_inspectedBT.DesignContainer, _inspectedBT.Scheduler);
            _searchWindow = CreateNodeSearchWindow(_graphView);
            _toolbar = CreateToolbar();

            rootVisualElement.Add(_graphView);
            rootVisualElement.Add(_toolbar);
        }

        private BTGraphView CreateGraphView(BTGraphDesign designContainer, BTScheduler scheduler)
        {
            var graphView = new BTGraphView(designContainer);

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                graphView.AttachVisualDebugger(scheduler);
            }

            graphView.StretchToParentSize();
            return graphView;
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();          

            var saveBtn = new Button(() => 
            { 
                _inspectedBT.DesignContainer.Save();
            }) { text = "Save" };

            var cleanupBtn = new Button(() => 
            { 
                _inspectedBT.DesignContainer.Cleanup();
            }) { text = "Cleanup" };

            var settingsBtn = new Button(() => 
            {
                _graphView.OpenGraphSettingsWnd();
            }) { text = "Settings" };

            var playgroundModeToggle = new Toggle() { text = "Playground Mode", value = BTGlobalSettings.Instance.PlaygroundMode };
            playgroundModeToggle.RegisterValueChangedCallback(evt => BTGlobalSettings.Instance.PlaygroundMode = evt.newValue);

            toolbar.Add(saveBtn);
            toolbar.Add(cleanupBtn);
            toolbar.Add(settingsBtn);
            toolbar.Add(playgroundModeToggle);

            return toolbar;
        }

        private BTNodeSearchWindow CreateNodeSearchWindow(BTGraphView graphView)
        {
            var window = ScriptableObject.CreateInstance<BTNodeSearchWindow>();
            graphView.nodeCreationRequest += context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), window);

            window.Init((nodeType, pos) => 
            {
                Vector2 worldMousePos = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, pos - position.position);
                Vector2 localMousePos = graphView.WorldToLocal(worldMousePos);
                Node node = BTGraphNodeFactory.CreateDefaultGraphNode(
                    nodeType, 
                    _graphView.GetBlackboard() as GraphBlackboard, 
                    localMousePos,
                    graphView.GraphDesign.TaskCtor);
                graphView.AddNode(node, localMousePos);
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

            if (_graphView != null)
            {
                _graphView.Cleanup();
                rootVisualElement.Remove(_graphView);
            }

            if (_toolbar != null)
            {
                rootVisualElement.Remove(_toolbar);
            }
        }
    }
}
