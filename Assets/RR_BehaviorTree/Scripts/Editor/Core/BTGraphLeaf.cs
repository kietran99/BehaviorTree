using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTGraphLeaf<T> : IBTGraphNodeInfo where T : BTBaseTask
    {
        private BTBaseTask _task;

        public string Name => _task.Name;

        public BTNodeType NodeType => BTNodeType.Leaf;

        public (BTPortCapacity In, BTPortCapacity Out) Capacity => (BTPortCapacity.Single, BTPortCapacity.None);

        public BTBaseTask Task => _task;

        public BTGraphLeaf()
        {
            _task = GetOrCreateTask();
        }

        private BTBaseTask GetOrCreateTask()
        {
            var taskReferences = Resources.Load<BTTaskReferenceContainer>(BTTaskReferenceContainer.TASK_REF_CONTAINER_PATH);
            var (task, isNull) = taskReferences.GetTask<T>();

            if (!isNull)
            {
                return task;
            }

            var path = UnityEditor.EditorUtility.SaveFilePanel("Save task", "Assets", $"{typeof(T).Name}.asset", "asset");

            if (string.IsNullOrEmpty(path))
            {
                return taskReferences.NullTask;
            }

            task = ScriptableObject.CreateInstance<T>();
            var relativePath = GetRelativePath(path, "Assets");
            UnityEditor.AssetDatabase.CreateAsset(task, relativePath);
            taskReferences.AddTask(task as BTBaseTask);
            Debug.Log($"{typeof(T).Name} was created!");
            return task;
        } 

        private string GetRelativePath(string absolutePath, string startDir)
        {
            for (int i = 0; i < absolutePath.Length; i++)
            {
                if (absolutePath.Substring(i, startDir.Length).Equals(startDir))
                {
                    return absolutePath.Substring(i);
                }
            }

            return string.Empty;
        }
    }
}
