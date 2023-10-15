using UnityEngine;
using UnityEditor.Experimental.GraphView;

using RR.Utils;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTGraphDebugNode
    {
        private static Color DEFAULT_EDGE_COLOR = ColorExtension.Create(146f);
        private static Color DEBUG_ACTIVE_EDGE_COLOR = new Color(222f / 255f, 240f/ 255f, 61f / 255f);
        private static Color DEBUG_INACTIVE_EDGE_COLOR = new Color(158f / 255f, 202f/ 255f, 255f / 255f, .2f);
        private const string CLASS_ACTIVE_BORDER = "active-border";

        private BTGraphNodeBase _decoratee;
        private Port _inPort;
        private Edge _inEdge;
        private BTGraphNodeBase _parentNode;
        private int _parentIdx;

        public BTGraphNodeBase ParentNode => _parentNode;
        public int ParentIdx => _parentIdx;

        public BTGraphDebugNode(BTGraphNodeBase decoratee)
        {
            _decoratee = decoratee;
            _decoratee.styleSheets.Add(StylesheetUtils.Load(nameof(BTGraphDebugNode)));

            if (_decoratee.inputContainer.childCount != 0)
            {
                _inPort = _decoratee.inputContainer[0] as Port;
                foreach (var edge in _inPort.connections) // There should only be 1 edge
                {
                    _inEdge = edge;
                    _parentNode = edge.output.node as BTGraphNodeBase;
                    _parentIdx = _parentNode.OrderValue;
                    // edge.capabilities &= ~Capabilities.Selectable;
                }
            }
        }

        public void Reset()
        {
            _decoratee.RemoveFromClassList(CLASS_ACTIVE_BORDER);
            _inPort.portColor = DEFAULT_EDGE_COLOR;           
            var edgeColor = DEBUG_INACTIVE_EDGE_COLOR;
            _inEdge.edgeControl.edgeWidth = 3;
            _inEdge.edgeControl.inputColor = edgeColor;
            _inEdge.edgeControl.outputColor = edgeColor;
            _inEdge.edgeControl.fromCapColor = _inEdge.edgeControl.inputColor;
            _inEdge.edgeControl.toCapColor = _inEdge.edgeControl.outputColor;
        }

        public void Tick()
        {
            _decoratee.AddToClassList(CLASS_ACTIVE_BORDER);
            _inPort.portColor = DEFAULT_EDGE_COLOR;
            var edgeColor = DEBUG_ACTIVE_EDGE_COLOR;
            _inEdge.edgeControl.edgeWidth = 6;
            _inEdge.edgeControl.inputColor = edgeColor;
            _inEdge.edgeControl.outputColor = edgeColor;
            _inEdge.edgeControl.fromCapColor = _inEdge.edgeControl.inputColor;
            _inEdge.edgeControl.toCapColor = _inEdge.edgeControl.outputColor;
        }

        public bool IsChildIdx(int idx)
        {
            var parentOutPort = _decoratee.outputContainer[0] as Port;
            foreach (var edge in parentOutPort.connections)
            {
                var child = edge.input.node as BTGraphNodeBase;

                if (child.OrderValue == idx)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
