namespace RR.AI
{
    public struct BBAddEntryEvent<T> : IBBEvent
    {
        public readonly string key;
        public readonly T value;

        public BBAddEntryEvent(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
