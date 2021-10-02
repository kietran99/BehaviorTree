namespace RR.AI
{
    [System.Serializable]
    public class BBString : BBValue<string>
    {
        public override string ValueTypeString => "String";

        protected override UnityEngine.UIElements.BaseField<string> PrimitivePropView => new UnityEngine.UIElements.TextField();
    }
}
