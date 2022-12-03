using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNode<T> : BTGraphNodeBase where T : IBTGraphNodeInfo, new()
    {
        private static Vector2 DEFAULT_NODE_SIZE = new Vector2(800f, 400f);

        // public static OnEdgeDrag

        protected T _nodeAction;
        protected string _name;
        protected string _description;

        public override string Name => _name;
        protected virtual BTTaskBase Task => null;

        public BTGraphNode(BTGraphInitParamsNode initParams)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/BTGraphNode"));
            AddToClassList("bold-text");
            mainContainer.style.minWidth = 100;
            
            _nodeAction = new T();
            CreatePorts(inputContainer, outputContainer, _nodeAction.Capacity.In, _nodeAction.Capacity.Out);
            _name = string.IsNullOrEmpty(initParams.name) ? _nodeAction.Name : initParams.name;
            _description = initParams.desc;
            _guid = string.IsNullOrEmpty(initParams.guid) ? System.Guid.NewGuid().ToString() : initParams.guid;
            
            titleContainer.Clear();
            StylizeTitleContainer(titleContainer);
            
            Texture2D titleIcon = initParams.icon;
            var titleContent = CreateTitleContent(_name, titleIcon);
            titleContainer.Add(titleContent);

            var pos = initParams.pos;
            SetPosition(new Rect(pos, DEFAULT_NODE_SIZE));
            x = Mathf.FloorToInt(pos.x);
            y = Mathf.FloorToInt(pos.y);

            OpenDecoSearchWnd = initParams.OpenDecoSearchWindow;
            OpenServiceSearchWnd = initParams.OpenServiceSearchWindow;

            if (_nodeAction.NodeType == BTNodeType.Root)
            {
                capabilities &= ~Capabilities.Movable;
                capabilities &= ~Capabilities.Deletable;
            }

            RegisterCallback<PointerDownEvent>(OnMouseDown);
            RegisterCallback<PointerMoveEvent>(OnMouseMove);
        }

        private void OnMouseMove(PointerMoveEvent evt) // No idea why this callback is invoked on mouse up
        {
            OrderLabel.SetEnabled(true);
            OrderLabel.visible = true;
        }

        private void OnMouseDown(PointerDownEvent evt)
        {
            OrderLabel.visible = false;
            OrderLabel.SetEnabled(false);
        }

        public BTGraphNode(Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid="", Texture2D icon = null)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/BTGraphNode"));
            AddToClassList("bold-text");
            mainContainer.style.minWidth = 100;
            
            _nodeAction = new T();
            CreatePorts(inputContainer, outputContainer, _nodeAction.Capacity.In, _nodeAction.Capacity.Out);
            _name = string.IsNullOrEmpty(name) ? _nodeAction.Name : name;
            _description = desc;
            _guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            
            titleContainer.Clear();
            StylizeTitleContainer(titleContainer);
            
            Texture2D titleIcon = icon;
            var titleContent = CreateTitleContent(_name, titleIcon);
            titleContainer.Add(titleContent);

            SetPosition(new Rect(pos, DEFAULT_NODE_SIZE));
            y = Mathf.FloorToInt(pos.y);

            if (_nodeAction.NodeType == BTNodeType.Root)
            {
                capabilities &= ~Capabilities.Movable;
                capabilities &= ~Capabilities.Deletable;
            }

            // BTBaseNode.OnRootTick += OnRootTick;
            // BTBaseNode.OnTick += OnNodeTick;
        }

        public override void OnSelected()
        {
            // BTGraphView.OnNodeSelected?.Invoke(_guid);
            BTGraphView.OnNewNodeSelected?.Invoke(_guid, _name, _description, Task);
            base.OnSelected();
            OrderLabel.BringToFront();
        }

        public override void OnUnselected()
        {
            BTGraphNodeAttacher.OnNodeUnselected(_guid);
            base.OnUnselected();
        }

        private void StylizeTitleContainer(VisualElement container)
        {
            container.style.justifyContent = Justify.Center;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
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
    
        public override void OnCreate(BTGraphDesign designContainer, Vector2 position)
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

        public override void OnDelete(BTGraphDesign designContainer)
        {
            BTSerializableNodeData nodeToDelete = designContainer.NodeDataList.Find(node => node.Guid == _guid);
            designContainer.NodeDataList.Remove(nodeToDelete);
        }

        public override void OnMove(BTGraphDesign designContainer, Vector2 moveDelta)
        {
            designContainer.NodeDataList.Find(node => node.Guid == _guid).Position = GetPosition().position;
            SyncOrderLabelPosition(moveDelta);
        }

        protected void SyncOrderLabelPosition(Vector2 moveDelta)
        {
            Rect position = GetPosition();
            OrderLabel.Move(moveDelta);
            OrderLabel.BringToFront();
        }

        public override void OnConnect(BTGraphDesign designContainer, string parentGuid)
        {
            designContainer.NodeDataList.Find(node => node.Guid == _guid).ParentGuid = parentGuid;
        }

        protected override bool AreAttachersAllowed => _nodeAction.NodeType != BTNodeType.Root;
    }
}
