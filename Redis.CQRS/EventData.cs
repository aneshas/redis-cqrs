namespace Redis.CQRS
{
    public class EventData
    {
        public long Id { get; }

        public object Event { get; }

        public void Ack() { }

        internal EventData(long id, object @event)
        {
            Id = id;
            Event = @event;
        }

        public EventData(object @event)
        {
            Event = @event;
        }

        private EventData() { }

        // Add metadata
        // Implement implicit conversion to T ?? does it make sense?
    }
}