using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using System.Linq;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNode<T> : BTGraphNodeBase where T : IBTGraphNodeInfo, new()
    {
        private static Vector2 DEFAULT_NODE_SIZE = new Vector2(800f, 400f);

        private readonly BTGraphNodeContent _mainContent;
        protected T _nodeAction;        
        protected string _description;
        
        protected override bool AreAttachersAllowed => _nodeAction.NodeType != BTNodeType.Root;

        protected override string MainContentStyleClassName => "composite";

        protected virtual BTTaskBase Task => null;

        public override string NodeName => _mainContent.Title;

        public BTGraphNode(BTGraphInitParamsNode initParams) : base()
        {
            styleSheets.Add(StylesheetUtils.Load("BTGraphNode"));
            AddToClassList("bold-text");
            mainContainer.style.minWidth = 100;
            
            _nodeAction = new T();
            _description = initParams.desc;
            _guid = string.IsNullOrEmpty(initParams.guid) ? System.Guid.NewGuid().ToString() : initParams.guid;
            CreatePorts(inputContainer, outputContainer, _nodeAction.Capacity.In, _nodeAction.Capacity.Out);
            
            titleContainer.Clear();
            titleContainer.styleSheets.Add(StylesheetUtils.Load("BTGraphNodeContainer"));
            _mainContent = new BTGraphNodeContent(string.IsNullOrEmpty(initParams.name) ? _nodeAction.Name : initParams.name, initParams.icon, MainContentStyleClassName);
            titleContainer.Add(_mainContent);

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
        }

        public override void OnSelected()
        {
            BTGraphView.OnNewElementSelected?.Invoke(new ElementSelectParams()
                {
                    Guid = _guid,
                    Name = NodeName,
                    Desc = _description,
                    Task = Task
                });
            base.OnSelected();
        }

        public override void OnUnselected()
        {
            BTGraphNodeAttacher.OnNodeUnselected(_guid);
            base.OnUnselected();
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
    
        public override void OnCreate(BTGraphDesign graphDesign, Vector2 position)
        {
            graphDesign.NodeDataList.Add(
                new BTSerializableNodeData(
                    position, NodeName, _description, _guid, GetParentGuid(inputContainer), _nodeAction.NodeType));
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
        }

        public override void OnConnect(BTGraphDesign designContainer, string parentGuid)
        {
            designContainer.NodeDataList.Find(node => node.Guid == _guid).ParentGuid = parentGuid;
        }

        public override void Rename(string name)
        {
            _mainContent.Rename(name);
        }
    }
}
