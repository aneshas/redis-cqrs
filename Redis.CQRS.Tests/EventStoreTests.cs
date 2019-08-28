using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using Tactical.DDD;
using Tactical.DDD.Testing;

namespace Redis.CQRS.Tests
{
    [TestClass]
    public class EventStoreTests
    {
        private readonly EventStore<IDomainEvent> _eventStore;

        public EventStoreTests()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost");
            _eventStore = new EventStore<IDomainEvent>(connectionMultiplexer);
        }

        [TestMethod]
        public async Task TestLoadingSavedEvents()
        {
            var stream = "FooAggregate";
            var aggregateId = "id0123";

            // TODO - Keep this overload for use with no metadata
            await _eventStore.SaveAsync(stream, aggregateId, 0,  new IDomainEvent[]
            {
                new FooEvent(DateTime.MaxValue, aggregateId, 24),
                new BarEvent(DateTime.MinValue, aggregateId, 38),
            });

            // This looks cumbersome ?!
            // We can use our DomainEvent base class instead of IDomainEvent - does this help? - try it
            var evts = new List<EventData<IDomainEvent>>
            {
                EventData.From(
                    new Dictionary<string, object>
                    {
                        {"user_ip", "127.0.0.1"}
                    },
                    new FooEvent(DateTime.MaxValue, aggregateId, 24) as IDomainEvent
                ),
                EventData.From(
                    new Dictionary<string, object>
                    {
                        {"user_ip", "127.0.0.2"}
                    },
                    new BarEvent(DateTime.MinValue, aggregateId, 38) as IDomainEvent
                )
            };

            var events = (await _eventStore.LoadAsync(stream, aggregateId)).ToArray();

            Assert.AreEqual(2, events.Length);

            var fooEvent = events[0].Event as FooEvent;

            Assert.AreEqual(1, events[0].Id);

            Assert.IsNotNull(fooEvent);

            Assert.AreEqual(DateTime.MaxValue, fooEvent.CreatedAt);
            Assert.AreEqual(aggregateId, fooEvent.AggregateId);
            Assert.AreEqual(24, fooEvent.FooField);

            var barEvent = events[1].Event as BarEvent;

            Assert.AreEqual(2, events[1].Id);

            Assert.IsNotNull(barEvent);

            Assert.AreEqual(DateTime.MinValue, barEvent.CreatedAt);
            Assert.AreEqual(aggregateId, barEvent.AggregateId);
            Assert.AreEqual(38, barEvent.BarField);
        }

        // Test concurrency exception

        // Test metadata

        // Test subscription
    }
}
