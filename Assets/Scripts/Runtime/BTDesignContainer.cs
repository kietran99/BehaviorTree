using UnityEngine;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    [CreateAssetMenu(fileName = "BT_Design_Container", menuName = "Generator/AI/BT Design Container")]
    public class BTDesignContainer : ScriptableObject
    {
        public List<BTNodeData> nodeDataList;
        public List<BTTaskData> taskDataList;

        public void Save(UnityEngine.UIElements.UQueryState<UnityEditor.Experimental.GraphView.Node> nodes)
        {
            nodeDataList.Clear();
            taskDataList.Clear();
            nodes.ForEach(node => (node as IBTSavable).Save(this));
        }
    }
}
