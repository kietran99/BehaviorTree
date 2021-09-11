using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI
{
	public interface IBBEntry
	{
		System.Type ValueType { get; }
		string ValueTypeString { get; }

		VisualElement CreatePropField();
	}

	public static class BBEntryFactory
	{
		private static System.Collections.Generic.Dictionary<System.Type, System.Func<IBBEntry>> _map = 
		new System.Collections.Generic.Dictionary<System.Type, System.Func<IBBEntry>>()
		{
			{ typeof(int), () => new BBInt() } 
		};

		public static IBBEntry New<T>(T value)
		{
			if (!_map.TryGetValue(typeof(T), out var ctor))
			{
				UnityEngine.Debug.Log($"Invalid Blackboard entry type {typeof(T)}");
				return default;
			}

			var entry = ctor() as BlackboardItem<T>;
			entry.Value = value;
			return entry;
		}
	}

	public abstract class BlackboardItem<T> : IBBEntry
	{
		private T _value;

        public T Value { get => _value; set => _value = value; }
		public System.Type ValueType => typeof(T);

        public abstract string ValueTypeString { get; }

        public abstract VisualElement CreatePropField();
	}

    public class BBInt : BlackboardItem<int>
    {
        public override string ValueTypeString => "Int";

        public override VisualElement CreatePropField() => new IntegerField() { value = Value };
    }
}