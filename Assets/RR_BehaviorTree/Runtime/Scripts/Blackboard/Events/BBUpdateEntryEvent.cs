namespace RR.AI
{
    public struct BBUpdateEntryEvent<T> : IBBEvent
    {
        public readonly string key;
        public readonly T oldValue, newValue;

        public BBUpdateEntryEvent(string key, T oldValue, T newValue)
        {
            this.key = key;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }
}