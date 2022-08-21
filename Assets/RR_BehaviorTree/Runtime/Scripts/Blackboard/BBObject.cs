namespace RR.AI
{
    [System.Serializable]
    public class BBObject : BBSerializableValue<UnityEngine.Object>
    {
        public override string ValueTypeString => "Object";
    }
}
