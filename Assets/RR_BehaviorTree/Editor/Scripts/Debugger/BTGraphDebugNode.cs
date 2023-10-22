using UnityEngine;
using UnityEditor.Experimental.GraphView;

using RR.Utils;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTGraphDebugNode
    {
        private static Color DEFAULT_EDGE_COLOR = ColorExtension.Create(146f);
        private static Color DEBUG_ACTIVE_EDGE_COLOR = new(222f / 255f, 240f/ 255f, 61f / 255f);
        private static Color DEBUG_INACTIVE_EDGE_COLOR = new(158f / 255f, 202f/ 255f, 255f / 255f, .2f);
        private const string CLASS_ACTIVE_BORDER = "active-border";

        private readonly BTGraphNodeBase _decoratee;
        private readonly Port _inPort, _outPort;
        private readonly Edge _inEdge;
        private readonly BTGraphNodeBase _parentNode;
        private readonly int _parentIdx;

        public BTGraphNodeBase ParentNode => _parentNode;
        public int ParentIdx => _parentIdx;

        private readonly System.Action<object, object> SetInternalInEdgeWidth;
        
        public BTGraphDebugNode(BTGraphNodeBase decoratee)
        {
            _decoratee = decoratee;
            _decoratee.styleSheets.Add(StylesheetUtils.Load(nameof(BTGraphDebugNode)));

            if (_decoratee.inputContainer.childCount != 0)
            {
                _inPort = _decoratee.inputContainer[0] as Port;
                _outPort = _decoratee.outputContainer.childCount == 0 ? null : _decoratee.outputContainer[0] as Port;
                foreach (var edge in _inPort.connections) // There should only be 1 edge
                {
                    _inEdge = edge;
                    SetInternalInEdgeWidth = edge.GetType()
                        .GetField("m_EdgeWidth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .SetValue;
                    _parentNode = edge.output.node as BTGraphNodeBase;
                    _parentIdx = _parentNode.OrderValue;
                }
            }
        }

        public void Reset()
        {
            _decoratee.RemoveFromClassList(CLASS_ACTIVE_BORDER);
            SetEdgeColor(DEBUG_INACTIVE_EDGE_COLOR, 3);
        }

        public void Tick()
        {
            _decoratee.AddToClassList(CLASS_ACTIVE_BORDER);
            SetEdgeColor(DEBUG_ACTIVE_EDGE_COLOR, 6);
        }

        private void SetEdgeColor(Color color, int edgeWidth)
        {
            _inPort.portColor = color;
            if (_outPort != null)
            {
                _outPort.portColor = color;
            }

            SetInternalInEdgeWidth(_inEdge, edgeWidth);
            _inEdge.edgeControl.inputColor = color;
            _inEdge.edgeControl.outputColor = color;
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
