namespace RR.AI
{
    public struct BBDeleteEntryEvent : IBBEvent
    {
        public readonly string key;

        public BBDeleteEntryEvent(string key)
        {
            this.key = key;
        }
    }
}