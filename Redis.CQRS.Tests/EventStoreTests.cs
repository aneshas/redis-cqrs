using System;
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
            var aggregateId = "id1234567";

            await _eventStore.SaveAsync(stream, aggregateId, 0, new []
            {
                new EventData(new FooEvent(DateTime.MaxValue, aggregateId, 24)), 
                new EventData(new BarEvent(DateTime.MinValue, aggregateId, 38)),
            });

            var events = (await _eventStore.LoadAsync(stream, aggregateId)).ToArray();

            Assert.AreEqual(2, events.Length);

            Assert.IsTrue(events.Any(data =>
            {
                var e = data.Event as FooEvent;

                Assert.IsNotNull(e);

                Assert.AreEqual(DateTime.MaxValue, e.CreatedAt);
                Assert.AreEqual(aggregateId, e.AggregateId);
                Assert.AreEqual(24, e.FooField);

                return true;
            }));

            Assert.IsTrue(events.Any(data =>
            {
                var e = data.Event as BarEvent;

                Assert.IsNotNull(e);

                Assert.AreEqual(DateTime.MinValue, e.CreatedAt);
                Assert.AreEqual(aggregateId, e.AggregateId);
                Assert.AreEqual(38, e.BarField);

                return true;
            }));
        }

        // Test concurrency exception

        // Test subscription
    }
}
