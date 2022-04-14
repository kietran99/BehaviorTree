namespace RR.AI
{
    [System.Serializable]
    public class BBVector2 : BBValue<UnityEngine.Vector2>
    {
        public override string ValueTypeString => "Vector2";

        protected override UnityEngine.UIElements.BaseField<UnityEngine.Vector2> PrimitivePropView 
			=> new UnityEditor.UIElements.Vector2Field();
    }
}