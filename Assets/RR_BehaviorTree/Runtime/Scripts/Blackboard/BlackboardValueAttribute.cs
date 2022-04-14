using System;

namespace RR.AI
{
	public class BlackboardValueAttribute : UnityEngine.PropertyAttribute
	{
		public System.Type ValueType;

        public BlackboardValueAttribute(Type valueType)
        {
            ValueType = valueType;
        }
    }
}