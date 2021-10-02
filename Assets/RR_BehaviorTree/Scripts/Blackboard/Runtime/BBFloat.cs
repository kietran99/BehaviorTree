namespace RR.AI
{
    [System.Serializable]
    public class BBFloat : BBValue<float>
    {
        public override string ValueTypeString => "Float";

        protected override UnityEngine.UIElements.BaseField<float> PrimitivePropView => new UnityEditor.UIElements.FloatField() { value = Value };
    }
}