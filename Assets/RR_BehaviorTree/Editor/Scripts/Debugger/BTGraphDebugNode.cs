using UnityEngine;
using UnityEditor.Experimental.GraphView;

using RR.Utils;

namespace RR.AI.BehaviorTree.Debugger
{
    public class BTGraphDebugNode
    {
        private static Color DEFAULT_EDGE_COLOR = ColorExtension.Create(146f);
        private static Color DEBUG_ACTIVE_EDGE_COLOR = new Color(222f / 255f, 240f/ 255f, 61f / 255f);
        private static Color DEBUG_INACTIVE_EDGE_COLOR = new Color(158f / 255f, 202f/ 255f, 255f / 255f, .2f);

        private BTGraphNodeBase _decoratee;

        public BTGraphDebugNode(BTGraphNodeBase decoratee)
        {
            _decoratee = decoratee;
        }

        public void Reset()
        {
            if (_decoratee.inputContainer.childCount == 0)
            {
                return;
            }

            var color = DEBUG_INACTIVE_EDGE_COLOR;
            var port = (_decoratee.inputContainer[0] as Port);
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

        public void Tick()
        {
            if (_decoratee.inputContainer.childCount == 0)
            {
                return;
            }

            var color = DEBUG_ACTIVE_EDGE_COLOR;
            var port = (_decoratee.inputContainer[0] as Port);
            port.portColor = DEFAULT_EDGE_COLOR;

            foreach (var edge in port.connections)
            {
                edge.edgeControl.edgeWidth = 6;
                edge.edgeControl.inputColor = color;
                edge.edgeControl.outputColor = color;
                edge.edgeControl.fromCapColor = edge.edgeControl.inputColor;
                edge.edgeControl.toCapColor = edge.edgeControl.outputColor;
            }
        }
    }
}
