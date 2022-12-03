namespace RR.AI.BehaviorTree
{
    public class BTRuntimeNodeBase
    {
        public BTRuntimeNodeBase(string guid, int successIdx, int failIdx, BTNodeType type, BTTaskBase task)
        {
            Guid = guid;
            SuccessIdx = successIdx;
            FailIdx = failIdx;
            Type = type;
            Task = task;
        }

        public string Guid { get; }
        public int SuccessIdx { get; }
        public int FailIdx { get; }
        public BTNodeType Type { get; }
        public BTTaskBase Task { get; }
        public BTRuntimeAttacher[] Decorators { get; set; }
        public BTRuntimeAttacher[] Services { get; set; }

        public int ProgressIdx => SuccessIdx > FailIdx ? SuccessIdx : FailIdx;

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            string taskNameIfValid = Task == null ? string.Empty : Task.Name;
            builder.AppendLine($"{Guid} - {Type}: ({SuccessIdx} : {FailIdx}) {taskNameIfValid}");

            if (Decorators != null)
            {
                builder.Append("Decorators: [");
                foreach (var deco in Decorators)
                {
                    builder.Append(deco.Task.Name);
                    builder.Append(" ");
                }
                builder.Append("]");
            }

            builder.AppendLine();

            if (Services != null)
            {
                builder.Append("Services: [");
                foreach (var service in Services)
                {
                    builder.Append(service.Task.Name);
                    builder.Append(" ");
                }
                builder.Append("]");
            }

            return builder.ToString();
        }
    }
}
