using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskReferenceContainer : ScriptableObject
    {
        [SerializeField]
        private System.Collections.Generic.List<BTBaseTask> _taskReferences = new System.Collections.Generic.List<BTBaseTask>();

        [SerializeField]
        private BTBaseTask _nullTask = null;

        public (BTBaseTask task, bool isNull) GetTask<T>()
        {
            _taskReferences.RemoveAll(item => item == null);

            for (int i = 0; i < _taskReferences.Count; i++)
            {
                if (_taskReferences[i].GetType() == typeof(T))
                {
                    return (_taskReferences[i], false);
                }
            }

            return (_nullTask, true);
        }

        public void AddTask(BTBaseTask task)
        {
            _taskReferences.Add(task);
        }

        public const string TASK_REF_CONTAINER_PATH = "BT_Tasks/BTRefsTask";

        public BTBaseTask NullTask => _nullTask;
    }
}
