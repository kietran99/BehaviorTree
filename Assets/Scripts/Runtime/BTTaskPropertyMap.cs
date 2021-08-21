namespace RR.AI.BehaviorTree
{
	[System.Serializable]
	public class BTTaskPropertyMap<TProp> : RR.Serialization.SerializableDictionary<string, TProp> where TProp : class, new()
	{
	}
}