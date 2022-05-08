using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using RR.Serialization;

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
        private SerializableDictionary<string, List<BTSerializableDecoData>> _decoratorDict = null;

        [SerializeField]
        private Blackboard _blackboard = null;

        [SerializeField]
        private SerializableDictionary<int, BTBaseTask> _taskDict = null;

        public List<BTSerializableNodeData> NodeDataList => _nodeDataList;
        public List<BTSerializableTaskData> TaskDataList => _taskDataList;
        public Blackboard Blackboard => _blackboard;

        public List<BTSerializableNodeDataBase> AsNodeDataBaseList =>
            _nodeDataList.Cast<BTSerializableNodeDataBase>()
            .Concat(_taskDataList.Cast<BTSerializableNodeDataBase>())
            .ToList();

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
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
                AssetDatabase.AddObjectToAsset(task, this);
			    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(task));
                _taskDict.Add(key, task);
            }
 
            return task;
        }

        public BTBaseTask CreateDummyTask(System.Type taskType)
            => CreateInstance(taskType) as BTBaseTask;

        public void ConnectNodes(string parentGuid, string childGuid)
        {
            _nodeDataList.Find(node => node.Guid == childGuid).ParentGuid = parentGuid;
        }

        public void DeleteNode(string guid)
        {
            BTSerializableNodeData nodeToDelete = _nodeDataList.Find(node => node.Guid == guid);
            _nodeDataList.Remove(nodeToDelete);
        }

        public void AddDecorator(string decorateeGuid, BTSerializableDecoData decorator)
        {
            if (!_decoratorDict.TryGetValue(decorateeGuid, out List<BTSerializableDecoData> decorators))
            {
                _decoratorDict.Add(decorateeGuid, new List<BTSerializableDecoData>() { decorator });
                return;
            }

            decorators.Add(decorator);
        }

        public bool TryGetDecorators(string decorateeGuid, out List<BTSerializableDecoData> decorators)
        {
            return _decoratorDict.TryGetValue(decorateeGuid, out decorators);
        }
    }
}