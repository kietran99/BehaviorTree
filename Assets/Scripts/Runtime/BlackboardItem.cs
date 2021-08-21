namespace RR.AI
{
	public interface IBlackboardItem
	{}

	public class BlackboardItem<T> : IBlackboardItem
	{
		private T _value;

        public BlackboardItem(T value)
        {
            _value = value;
        }

        public T Value { get => _value; set => _value = value; }
		public string TypeName => typeof(T).ToString();
	}
}