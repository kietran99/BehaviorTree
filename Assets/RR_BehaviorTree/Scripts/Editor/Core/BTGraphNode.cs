using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace RR.AI.BehaviorTree
{
    public interface IBTGraphNode : IBTSerializableNode
    {
        void OnRemove();
    }

    public class BTGraphNode<T> : Node, IBTGraphNode where T : IBTGraphNodeInfo, new()
    {
        private static Vector2 DEFAULT_NODE_SIZE = new Vector2(600f, 400f);
        private static Color DEFAULT_EDGE_COLOR = new Color(146f / 255f, 146f/ 255f, 146f / 255f);
        private static Color DEBUG_ACTIVE_EDGE_COLOR = new Color(222f / 255f, 240f/ 255f, 61f / 255f);
        private static Color DEBUG_INACTIVE_EDGE_COLOR = new Color(158f / 255f, 202f/ 255f, 255f / 255f, .2f);

        // public static OnEdgeDrag

        protected T _nodeAction;
        protected string _name;
        protected string _description;
        protected string _guid;
        public string Guid => _guid;

        public BTGraphNode(Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid="", Texture2D icon = null)
        {
            styleSheets.Add(Resources.Load<UnityEngine.UIElements.StyleSheet>("Stylesheets/BTGraphNode"));
            AddToClassList("bold-text");
            mainContainer.style.minWidth = 100;

            _nodeAction = new T();
            CreatePorts(inputContainer, outputContainer, _nodeAction.Capacity.In, _nodeAction.Capacity.Out);
            _name = string.IsNullOrEmpty(name) ? _nodeAction.Name : name;
            _description = desc;
            _guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            
            titleContainer.Clear();
            StylizeTitleContainer(titleContainer);
            
            var container = CreateTitleContent(_nodeAction.NodeType, icon);
            titleContainer.Add(container);
            
            SetPosition(new Rect(pos, DEFAULT_NODE_SIZE));

            if (_nodeAction.NodeType == BTNodeType.Root)
            {
                capabilities &= ~Capabilities.Movable;
                capabilities &= ~Capabilities.Deletable;
            }

            BTBaseNode.OnRootTick += OnRootTick;
            BTBaseNode.OnTick += OnNodeTick;
            // var label = new Label("0");
            // label.style.fontSize = 15;
            // titleButtonContainer.Add(label);
            // Debug.Log(_nodeAction.Name);

            // if (_nodeAction.NodeType != BTNodeType.Leaf)
            // {
            //     (outputContainer[0] as Port).RegisterCallback<UnityEngine.UIElements.MouseUpEvent>(OnMouseUp);
            // }
        }

        public override void OnSelected()
        {
            // BTGraphView.OnNodeSelected?.Invoke(_guid);
            BTGraphView.OnNewNodeSelected?.Invoke(_guid, _name, _description, null);
            base.OnSelected();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
        }

        // private void OnMouseUp(MouseUpEvent evt)
        // {
            // Debug.Log((outputContainer[0] as Port).connected);
            // Debug.Log((outputContainer[0] as Port).ContainsPoint(evt.localMousePosition));
            // Debug.Log(evt.mousePosition);
            // Debug.Log(evt.originalMousePosition);
        // }

        private void OnRootTick(string _)
        {
            if (inputContainer.childCount == 0)
            {
                return;
            }

            var color = DEBUG_INACTIVE_EDGE_COLOR;
            var port = (inputContainer[0] as Port);
            port.portColor = DEFAULT_EDGE_COLOR;

            foreach (var edge in port.connections)
            {
                edge.edgeControl.edgeWidth = 3;
                edge.edgeControl.inputColor = color;
                edge.edgeControl.outputColor = color;
                edge.edgeControl.fromCapColor = edge.edgeControl.inputColor;
                edge.edgeControl.toCapColor = edge.edgeControl.outputColor;
            }
        }

        private void OnNodeTick(string guid)
        {
            if (_guid != guid)
            {
                return;
            }
            
            if (inputContainer.childCount == 0)
            {
                return;
            }

            var port = (inputContainer[0] as Port);
            var color = DEBUG_ACTIVE_EDGE_COLOR;
            port.portColor = color;
            
            foreach (var edge in port.connections)
            {
                edge.edgeControl.edgeWidth = 5;
                edge.edgeControl.inputColor = color;
                edge.edgeControl.outputColor = color;
                edge.edgeControl.fromCapColor = edge.edgeControl.inputColor;
                edge.edgeControl.toCapColor = edge.edgeControl.outputColor;
            }
        }

        private void StylizeTitleContainer(VisualElement container)
        {
            container.style.justifyContent = Justify.Center;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
        }

        private VisualElement CreateTitleContent(BTNodeType type, Texture2D nodeIcon = null)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            var icon = new Image();
            icon.image = type != BTNodeType.Leaf ? GetIcon(type) : nodeIcon;
            icon.scaleMode = ScaleMode.ScaleToFit;
            icon.style.marginRight = 5;
            container.Add(icon);

            var titleLabel = new Label(_name);
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.fontSize = 14;
            titleLabel.style.color = Color.white;
            container.Add(titleLabel);

            return container;
        }

        protected virtual Texture2D GetIcon(BTNodeType type)
        {
            return Resources.Load<Texture2D>($"Icons/{GetIconFileName(type)}");
        }

        private string GetIconFileName(BTNodeType type)
        {
            switch (type)
            {
                case BTNodeType.Root:
                    return "root";
                case BTNodeType.Sequencer:
                    return "sequencer";
                case BTNodeType.Selector:
                    return "selector";
                default:
                    return string.Empty;
            }
        }

        private void CreatePorts(
            VisualElement inputContainer, VisualElement outputContainer, 
            BTPortCapacity inCapacity, BTPortCapacity outCapacity)
        {
            if (inCapacity != BTPortCapacity.None) 
            {
                CreateInputPort(inputContainer, string.Empty);
            }

            if (outCapacity != BTPortCapacity.None)
            {
                CreateOutputPort(outputContainer, string.Empty, 
                    outCapacity == BTPortCapacity.Single ? Port.Capacity.Single : Port.Capacity.Multi);
            }
        }

        private Port CreateInputPort(VisualElement inputContainer, string name="In")
        {
            var port = CreatePort(Direction.Input);
            port.portName = name;
            inputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        private Port CreateOutputPort(VisualElement outputContainer, string name="Out", Port.Capacity capacity=Port.Capacity.Single)
        {
            var port = CreatePort(Direction.Output, capacity);
            port.portName = name;
            outputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity=Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
        }
    
        public virtual void OnCreate(BTDesignContainer designContainer, UnityEngine.Vector2 position)
        {
            designContainer.NodeDataList.Add(
                new BTSerializableNodeData(
                    position, _name, _description, _guid, GetParentGuid(inputContainer), _nodeAction.NodeType));
        }

        protected string GetParentGuid(VisualElement inputContainer)
        {
            if (inputContainer.childCount == 0)
            {
                return string.Empty;
            }

            var inputPort = inputContainer[0] as Port;

            if (!inputPort.connected)
            {
                return string.Empty;
            }

            var parentNode = inputPort.connections.First(edge => edge.output != inputPort).output.node;
            return (parentNode as IBTSerializableNode).Guid;
        }

        public virtual void OnDelete(BTDesignContainer designContainer)
        {
            BTSerializableNodeData nodeToDelete = designContainer.NodeDataList.Find(node => node.Guid == _guid);
            designContainer.NodeDataList.Remove(nodeToDelete);
        }

        public void OnRemove()
        {
            BTBaseNode.OnTick -= OnNodeTick;
            BTBaseNode.OnRootTick -= OnRootTick;
        }
    }
}
