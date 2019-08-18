using System;
using Tactical.DDD;

namespace Redis.CQRS.Tests
{
    public sealed class BarEvent : IDomainEvent
    {
    public DateTime CreatedAt { get; set; }

        public string AggregateId { get; }

        public int BarField { get; }

        public BarEvent(DateTime createdAt, string aggregateId, int barField)
        {
            CreatedAt = createdAt;
            AggregateId = aggregateId;
            BarField = barField;
        }
    }
}
