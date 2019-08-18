using System;
using Tactical.DDD;

namespace Redis.CQRS.Tests
{
    public sealed class FooEvent : IDomainEvent
    {
        public DateTime CreatedAt { get; set; }

        public string AggregateId { get; }

        public int FooField { get; }

        public FooEvent(DateTime createdAt, string aggregateId, int fooField)
        {
            CreatedAt = createdAt;
            AggregateId = aggregateId;
            FooField = fooField;
        }
    }
}