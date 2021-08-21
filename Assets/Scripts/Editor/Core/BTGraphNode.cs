using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNode<T> : Node, IBTSavable where T : IBTGraphNodeInfo, new()
    {
        private static Vector2 _defaultNodeSize = new Vector2(300f, 400f);

        protected T _nodeAction;

        protected string _guid;
        public string Guid => _guid;

        public BTGraphNode(Vector2 pos, string guid="")
        {
            styleSheets.Add(Resources.Load<UnityEngine.UIElements.StyleSheet>("BTGraphNode"));
            AddToClassList("bold-text");

            _nodeAction = new T();
            CreatePorts(inputContainer, outputContainer, _nodeAction.Capacity.In, _nodeAction.Capacity.Out);
            _guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            title = _nodeAction.Name;
            SetPosition(new Rect(pos, _defaultNodeSize));

            if (_nodeAction.NodeType == BTNodeType.Root)
            {
                capabilities &= ~Capabilities.Movable;
                capabilities &= ~Capabilities.Deletable;
            }

            var label = new Label("0");
            label.style.fontSize = 15;
            titleButtonContainer.Add(label);
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
    
        public virtual void Save(BTDesignContainer designContainer)
        {
            designContainer.nodeDataList.Add(
                new BTSerializableNodeData(GetPosition().position, _guid, GetParentGuid(inputContainer), _nodeAction.NodeType));
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
            return (parentNode as IBTSavable).Guid;
        }
    }
}
