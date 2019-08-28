using System;
using System.Collections.Generic;
using System.Text;

namespace Redis.CQRS.Projections
{
    public class EventData<T> : CQRS.EventData<T>
        where T : class
    {
        public void Ack() { }

        internal EventData(long id, T @event) : base(id, @event)
        {
        }
    }
}
