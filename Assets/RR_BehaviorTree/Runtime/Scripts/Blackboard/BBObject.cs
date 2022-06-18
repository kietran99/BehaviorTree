using UnityEngine;

namespace RR.AI
{
    [System.Serializable]
    public class BBObject : BBValue<Object>
    {
        public override string ValueTypeString => "Object";

        protected override UnityEngine.UIElements.BaseField<Object> PrimitivePropView 
			=> new UnityEditor.UIElements.ObjectField() { objectType = typeof(Object) };
    }
}