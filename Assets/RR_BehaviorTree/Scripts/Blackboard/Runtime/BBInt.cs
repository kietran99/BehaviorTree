using UnityEngine.UIElements;

namespace RR.AI
{
    [System.Serializable]
    public class BBInt : BBValue<int>
    {
        public override string ValueTypeString => "Int";

        protected override BaseField<int> PrimitivePropView => new UnityEditor.UIElements.IntegerField() { value = Value };
    }
}
