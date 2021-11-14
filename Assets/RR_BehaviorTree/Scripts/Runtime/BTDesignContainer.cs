using UnityEngine;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    [CreateAssetMenu(fileName = "BT_Design_Container", menuName = "Generator/AI/BT Design Container")]
    public class BTDesignContainer : ScriptableObject
    {
        [SerializeField]
        private List<BTSerializableNodeData> _nodeDataList = null;

        [SerializeField]
        private List<BTSerializableTaskData> _taskDataList = null;

        [SerializeField]
        private Blackboard _blackboard = null;

        [SerializeField]
        private Serialization.SerializableDictionary<int, BTBaseTask> _taskDict = null;

        public List<BTSerializableNodeData> NodeDataList => _nodeDataList;
        public List<BTSerializableTaskData> TaskDataList => _taskDataList;
        public Blackboard Blackboard => _blackboard;

        public void Save(UnityEngine.UIElements.UQueryState<UnityEditor.Experimental.GraphView.Node> nodes)
        {
            _nodeDataList.Clear();
            _taskDataList.Clear();
            nodes.ForEach(node => (node as IBTSavable).Save(this));
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        public BTBaseTask GetOrCreateTask(System.Type taskType)
        {
            var key = Animator.StringToHash(taskType.ToString());

            if (!_taskDict.TryGetValue(key, out var task))
            {
                task = CreateInstance(taskType) as BTBaseTask;
                UnityEditor.AssetDatabase.AddObjectToAsset(task, this);
			    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(task));
                _taskDict.Add(key, task);
            }
 
            return task;
        }
    }
}
