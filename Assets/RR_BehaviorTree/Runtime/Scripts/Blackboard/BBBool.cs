using UnityEngine.UIElements;

namespace RR.AI
{
	[System.Serializable]
	public class BBBool : BBValue<bool>
	{
		public override string ValueTypeString => "Bool";

        protected override BaseField<bool> PrimitivePropView => new UnityEngine.UIElements.Toggle() { value = Value };
    }
}