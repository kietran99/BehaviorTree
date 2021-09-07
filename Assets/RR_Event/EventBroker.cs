namespace RR.Event
{
    public class EventBroker
    {
        private System.Collections.Generic.Dictionary<System.Type, IBaseEvent> _eventDict;

        public void Subscribe<T>(System.Action<T> listener) where T : IEventData
        {
            if (_eventDict.TryGetValue(typeof(T), out IBaseEvent publisher))
            {
                (publisher as GameEvent<T>).Add(listener);
                return;
            }
            
            publisher = new GameEvent<T>();
            _eventDict.Add(typeof(T), publisher);
            (publisher as GameEvent<T>).Add(listener);
        }

        public void Unsubscribe<T>(System.Action<T> listener) where T : IEventData
        {
            if (_eventDict.TryGetValue(typeof(T), out IBaseEvent publisher))
            {
                (publisher as GameEvent<T>).Remove(listener);
            }
        }

        public void Publish<T>(T eventData) where T : IEventData
        {
            if (_eventDict.TryGetValue(typeof(T), out IBaseEvent publisher))
            {
                (publisher as GameEvent<T>).Invoke(eventData);
            }
        }
    }
}
