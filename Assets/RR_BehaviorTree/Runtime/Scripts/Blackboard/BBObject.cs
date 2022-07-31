using UnityEngine;

namespace RR.AI
{
    [System.Serializable]
    public class BBObject : BBValue<Object>
    {
        public override string ValueTypeString => "Object";
    }
}