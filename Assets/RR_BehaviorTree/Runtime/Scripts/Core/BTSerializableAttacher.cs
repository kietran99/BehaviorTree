namespace RR.AI.BehaviorTree
{
	[System.Serializable]
	public class BTSerializableAttacher
	{
		public string guid;
		public string name;
		public BTBaseTask task;

        public BTSerializableAttacher(string guid, string name, BTBaseTask decorator)
        {
            this.guid = guid;
            this.name = name;
            this.task = decorator;
        }
    }
}