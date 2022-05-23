namespace RR.AI.BehaviorTree
{
	[System.Serializable]
	public class BTSerializableDecoData
	{
		public string guid;
		public string name;
		public BTBaseTask decorator;

        public BTSerializableDecoData(string guid, string name, BTBaseTask decorator)
        {
            this.guid = guid;
            this.name = name;
            this.decorator = decorator;
        }
    }
}