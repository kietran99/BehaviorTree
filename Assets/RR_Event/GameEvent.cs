namespace RR.Event
{
	public interface IBaseEvent {} 

	public class GameEvent<T> : IBaseEvent
	{
		private event System.Action<T> listeners;

        public void Add(System.Action<T> listener) => listeners += listener;

        public void Remove(System.Action<T> listener) => listeners -= listener;

        public void Invoke(T eventData) => listeners?.Invoke(eventData);
	}
}