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

        public void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        public void Cleanup()
        {
            RemoveRedundantBTTaskSOs();
            Save();
        }

        private void RemoveRedundantBTTaskSOs()
        {
            HashSet<int> allTaskIds = new HashSet<int>();

            foreach (BTSerializableTaskData taskData in _taskDataList)
            {
                var taskId = Animator.StringToHash(taskData.Task.GetType().ToString());
                allTaskIds.Add(taskId);
            }

            _taskDict.ForEach((id, taskSO) => 
            { 
                if (!allTaskIds.Contains(id))  
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(taskSO);
                }
            });

            _taskDict.RemoveAll((id, _) => allTaskIds.Contains(id));
        }

        public BTBaseTask GetOrCreateTask(System.Type taskType)
        {
            int key = Animator.StringToHash(taskType.ToString());

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
