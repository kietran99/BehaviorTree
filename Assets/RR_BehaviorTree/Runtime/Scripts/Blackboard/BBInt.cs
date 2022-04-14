namespace RR.AI
{
    [System.Serializable]
    public class BBInt : BBValue<int>
    {
        public override string ValueTypeString => "Int";

        protected override UnityEngine.UIElements.BaseField<int> PrimitivePropView => new UnityEditor.UIElements.IntegerField();
    }
}
