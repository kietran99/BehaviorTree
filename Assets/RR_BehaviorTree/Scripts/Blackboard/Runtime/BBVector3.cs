namespace RR.AI
{
    [System.Serializable]
    public class BBVector3 : BBValue<UnityEngine.Vector3>
    {
        public override string ValueTypeString => "Vector3";

        protected override UnityEngine.UIElements.BaseField<UnityEngine.Vector3> PrimitivePropView 
			=> new UnityEditor.UIElements.Vector3Field();
    }
}