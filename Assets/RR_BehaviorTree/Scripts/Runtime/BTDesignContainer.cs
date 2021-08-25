using UnityEngine;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    [CreateAssetMenu(fileName = "BT_Design_Container", menuName = "Generator/AI/BT Design Container")]
    public class BTDesignContainer : ScriptableObject
    {
        public List<BTSerializableNodeData> nodeDataList = new List<BTSerializableNodeData>();
        public List<BTTaskData> taskDataList = new List<BTTaskData>();

        public void Save(UnityEngine.UIElements.UQueryState<UnityEditor.Experimental.GraphView.Node> nodes)
        {
            nodeDataList.Clear();
            taskDataList.Clear();
            nodes.ForEach(node => (node as IBTSavable).Save(this));
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
