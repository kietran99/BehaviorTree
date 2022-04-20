namespace RR.AI.BehaviorTree
{
	[System.Serializable]
	public class BTSerializableDecoData
	{
		public string name;
		public BTBaseTask decorator;

        public BTSerializableDecoData(string name, BTBaseTask decorator)
        {
            this.name = name;
            this.decorator = decorator;
        }
    }
}