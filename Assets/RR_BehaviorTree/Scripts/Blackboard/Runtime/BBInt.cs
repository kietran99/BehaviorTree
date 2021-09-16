namespace RR.AI
{
    [System.Serializable]
    public class BBInt : BBValue<int>
    {
        public override string ValueTypeString => "Int";

        public override UnityEngine.UIElements.VisualElement CreatePropField() => new UnityEditor.UIElements.IntegerField() { value = Value };
    }
}
