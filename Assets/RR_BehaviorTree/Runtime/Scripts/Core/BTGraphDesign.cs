using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using RR.Serialization;

namespace RR.AI.BehaviorTree
{
    [CreateAssetMenu(fileName = "BT_Graph", menuName = "Generator/AI/BT Graph")]
    public class BTGraphDesign : ScriptableObject
    {
        [SerializeField]
        private List<BTSerializableNodeData> _nodeDataList = null;

        [SerializeField]
        private List<BTSerializableTaskData> _taskDataList = null;

        [SerializeField]
        private SerializableDictionary<string, List<BTSerializableAttacher>> _attacherDict = null;

        [SerializeField]
        private Blackboard _blackboard = null;

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
            Save();
        }

        public BTTaskBase TaskCtor(System.Type taskType)
        {
            var task = CreateInstance(taskType) as BTTaskBase;
            AssetDatabase.AddObjectToAsset(task, this);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(task));
            return task;
        }

        public BTTaskBase CreateDummyTask(System.Type taskType)
            => CreateInstance(taskType) as BTTaskBase;

        public BTNodeType FindParentType(string parentGuid)
            => _nodeDataList.Find(node => node.Guid == parentGuid).NodeType;

        public void ConnectNodes(string parentGuid, string childGuid)
        {
            _nodeDataList.Find(node => node.Guid == childGuid).ParentGuid = parentGuid;
        }

        public void DeleteNode(string guid)
        {
            BTSerializableNodeData nodeToDelete = _nodeDataList.Find(node => node.Guid == guid);
            _nodeDataList.Remove(nodeToDelete);
        }

        public bool DeleteAttacher(string nodeGuid, string attacherGuid)
        {
            if (!_attacherDict.TryGetValue(nodeGuid, out List<BTSerializableAttacher> attachers))
            {
                Debug.LogWarning($"Invalid node: {nodeGuid}");
                return false;
            }

            BTSerializableAttacher attacherToDelete = null;
            foreach (BTSerializableAttacher attacher in attachers)
            {
                if (attacher.guid != attacherGuid)
                {
                    continue;
                }

                attacherToDelete = attacher;
                break;
            }

            if (attacherToDelete == null)
            {
                Debug.LogWarning($"Invalid attacher: {attacherGuid}");
                return false;
            }

            attachers.Remove(attacherToDelete);
            return true;
        }

        public void AddAttacher(string decorateeGuid, BTSerializableAttacher attacher)
        {
            if (!_attacherDict.TryGetValue(decorateeGuid, out List<BTSerializableAttacher> attachers))
            {
                _attacherDict.Add(decorateeGuid, new List<BTSerializableAttacher>() { attacher });
                return;
            }

            attachers.Add(attacher);
        }

        public bool TryGetAttachers(string decorateeGuid, out List<BTSerializableAttacher> attachers)
        {
            return _attacherDict.TryGetValue(decorateeGuid, out attachers);
        }

        public bool TryGetAttachers(string decorateeGuid, out List<BTSerializableAttacher> decorators, out List<BTSerializableAttacher> services)
        {
            if (!_attacherDict.TryGetValue(decorateeGuid, out List<BTSerializableAttacher> attachers))
            {
                decorators = services = null;
                return false;
            }

            decorators = new List<BTSerializableAttacher>();
            services = new List<BTSerializableAttacher>();

            foreach (var attacher in attachers)
            {
                bool isDecorator = typeof(BTDecoratorSimpleBase).IsAssignableFrom(attacher.task.GetType());
                if (isDecorator)
                {
                    decorators.Add(attacher);
                }
                else
                {
                    services.Add(attacher);
                }
            }

            return true;
        }
    }
}
