using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTRuntimeAttacher
    {
        public BTRuntimeAttacher(string guid, BTTaskBase task)
        {
            Guid = guid;
            Task = task;
        }

        public string Guid { get; }
        public BTTaskBase Task { get; }

        public void Init(BTRuntimeContext context)
            => Task.Init(context);

        public BTNodeState Update()
            => Task.Update();
    }
}
