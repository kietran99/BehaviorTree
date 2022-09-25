namespace RR.AI
{
    public struct BBUpdateEntryEvent<T> : IBBEvent
    {
        public readonly string key;
        public readonly T value;

        public BBUpdateEntryEvent(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}