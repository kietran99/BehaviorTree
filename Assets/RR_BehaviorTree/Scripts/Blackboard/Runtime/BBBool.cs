namespace RR.AI
{
	[System.Serializable]
	public class BBBool : BBValue<bool>
	{
		public override string ValueTypeString => "Bool";

		public override UnityEngine.UIElements.VisualElement CreatePropField() => new UnityEngine.UIElements.Toggle() { value = Value };
	}
}