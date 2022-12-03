namespace RR.AI.BehaviorTree
{
	[System.Serializable]
	public class BTSerializableAttacher
	{
		public string guid;
		public string name;
		public BTTaskBase task;

        public BTSerializableAttacher(string guid, string name, BTTaskBase decorator)
        {
            this.guid = guid;
            this.name = name;
            this.task = decorator;
        }
    }
}