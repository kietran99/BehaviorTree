namespace RR.AI
{
	public class BlackboardValueAttribute : UnityEngine.PropertyAttribute
	{
		public System.Type ValueType;

        public BlackboardValueAttribute(System.Type valueType)
        {
            ValueType = valueType;
        }
    }
}