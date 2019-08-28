using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace Redis.CQRS
{
    public static class EventData {
        public static EventData<T> From<T>(Dictionary<string, object> metaData, T @event)
            where T : class
        {
            return new EventData<T>(@event);
        }
    }

    public class EventData<T> where T : class
    {
        public long Id { get; }

        public T Event { get; }

        private EventData() { }

        internal EventData(long id, T @event)
        {
            Id = id;
            Event = @event;
        }

        internal EventData(T @event)
        {
            Event = @event;
        }

        public static implicit operator T(EventData<T> eventData) => eventData.Event;
    }
}