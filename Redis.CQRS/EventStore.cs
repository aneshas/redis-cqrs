using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redis.CQRS
{
    public class EventStore<T> where T : class
    {
        private const string EventKey = "event_data";
        private const string EventStreamKey = "event_stream";
        private const string EventIdKey = "event_id";

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        // TODO Default to json serialization but leave an option to provide custom serializer

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStore(IConnectionMultiplexer redisConnectionMultiplexer)
        {
            _connectionMultiplexer = redisConnectionMultiplexer;
        }

        // TODO - Try adding status
        // Add doc comments
        public async Task SaveAsync(string aggregate, string aggregateId, uint version, IEnumerable<T> eventStream)
        {
            var db = _connectionMultiplexer.GetDatabase();

            var stream = StreamName(aggregate, aggregateId);

            var tasks = new List<Task>();

            // TODO - Do we need transaction ?

            foreach (var @event in eventStream)
            {
                var eventId = $"{++version}";
                var serializedEvent = JsonConvert.SerializeObject(@event, _jsonSerializerSettings);

                tasks.Add(db.StreamAddAsync(
                    stream,
                    EventKey,
                    serializedEvent,
                    messageId: eventId));

                tasks.Add(db.StreamAddAsync(AggregateStream(aggregate), new[]
                {
                    new NameValueEntry(EventStreamKey, stream),
                    new NameValueEntry(EventIdKey, eventId)
                }));
            }

            await Task.WhenAll(tasks);
        }

        private static string AggregateStream(string aggregate) => $"{aggregate.ToLower()}:all";

        public async Task<IEnumerable<T>> LoadAsync(string aggregate, string aggregateId)
        {
            var db = _connectionMultiplexer.GetDatabase();

            var streamEntries = await db.StreamReadAsync(StreamName(aggregate, aggregateId), "0-0");

            if (streamEntries == null) return new List<T>();

            return streamEntries.Select(entry
                => JsonConvert.DeserializeObject(entry.Values[0].Value, _jsonSerializerSettings) as T);
        }

        private static string StreamName(string aggregate, string aggregateId) =>
            $"{aggregate.ToLower()}:{aggregateId.ToLower()}";

        public Task SubscribeToStreamsAsync(string consumerGroup, int batchSize, string[] streams, Action<StreamEntry<T>> eventHandler)
        {
            return Task.CompletedTask;
        }

        // TODO Load events with just stream name overload

        // TODO Subscribe to all with ack
        // TODO Subscribe to stream with ack

        // TODO Choice between subscribing to certain aggregate or all aggregates ... 

        // TODO Subscriptions should reconnect
    }
}