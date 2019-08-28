using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redis.CQRS
{
    public class EventStore<T> where T : class
    {
        // We need to export these in a smarter way (since EventStore is generic)
        // Make this configurable (then it can also be used to namespace)
        private const string RedisPrefix = "redis_cqrs_";

        private const string EventKey =  "event_data";
        private const string EventStreamKey = "event_stream";
        private const string EventIdKey = "event_id";

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        // TODO Default to json serialization but leave an option to provide custom serializer

        // Add RedisPrefix to all of these keys?
        // debtor:some-debtor-id - aggregate event stream 
        // debtor:events_all - pointers to aggregate events (aggregate event stream and event id)
        // debtor:streams - set with all aggregate streams 

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStore(IConnectionMultiplexer redisConnectionMultiplexer)
        {
            _connectionMultiplexer = redisConnectionMultiplexer;
        }

        public async Task SaveAsync(string aggregate, string aggregateId, uint version, IEnumerable<EventData<T>> eventData)
        {

        }

        // Add doc comments
        public async Task SaveAsync(string aggregate, string aggregateId, uint version, IEnumerable<T> events)
        {
            if (events == null || !events.Any()) return;

            CheckAggregateArgs(aggregate, aggregateId);

            var db = _connectionMultiplexer.GetDatabase();

            var stream = AggregateStream(aggregate, aggregateId);

            var tasks = new List<Task>
            {
                // We don't test this for failure since it is a convenience
                db.SetAddAsync(AggregateStreamsSet(aggregate), stream)
            };

            // TODO - Do we need transaction ?

            foreach (var @event in events)
            {
                var eventId = $"{++version}";
                var serializedEvent = JsonConvert.SerializeObject(@event, _jsonSerializerSettings);

                tasks.Add(db.StreamAddAsync(
                    stream,
                    EventKey,
                    serializedEvent,
                    messageId: eventId));

                tasks.Add(db.StreamAddAsync(AggregateAllEventsStream(aggregate), new[]
                {
                    new NameValueEntry(EventStreamKey, stream),
                    new NameValueEntry(EventIdKey, eventId)
                }));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<EventData<T>>> LoadAsync(string aggregate, string aggregateId)
        {
            CheckAggregateArgs(aggregate, aggregateId);

            var db = _connectionMultiplexer.GetDatabase();

            var streamEntries = await db.StreamReadAsync(AggregateStream(aggregate, aggregateId), "0-0");

            if (streamEntries == null) return new List<EventData<T>>();

            return streamEntries.Select(entry =>
            {
                var @event = JsonConvert.DeserializeObject(entry.Values[0].Value, _jsonSerializerSettings) as T;

                var id = entry.Id.ToString().Split('-');

                return new EventData<T>(long.Parse(id[0]), @event);
            });
        }

        private static string AggregateStreamsSet(string aggregate) => 
            $"{RedisPrefix}{aggregate.ToLower()}:streams"; // TODO add constant

        private static string AggregateAllEventsStream(string aggregate) => 
            $"{RedisPrefix}{aggregate.ToLower()}:events_all"; // TODO add constant

        private static string AggregateStream(string aggregate, string aggregateId) =>
            $"{RedisPrefix}{aggregate.ToLower()}:{aggregateId.ToLower()}";

        private static void CheckAggregateArgs(string aggregate, string aggregateId)
        {
            if (string.IsNullOrWhiteSpace(aggregate) || string.IsNullOrWhiteSpace(aggregateId))
                throw new InvalidEnumArgumentException("aggregate and aggregateId cannot be null or whitespace");
        }

        //public Task SubscribeToStreamsAsync(string consumerGroup, int batchSize, string[] streams, Action<EventData<T>> eventHandler)
        //{
        //    return Task.CompletedTask;
        //}

        // TODO Load events with just stream name overload

        // TODO Subscribe to all with ack
        // TODO Subscribe to stream with ack

        // TODO Choice between subscribing to certain aggregate or all aggregates ... 

        // TODO Subscriptions should reconnect
        // TODO Subscriptions should keep retrying same event if throwing... 
    }
}