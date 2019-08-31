using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Redis.CQRS.Projections
{
    internal class ConsumerGroupReader<T> where T : class
    {
        private readonly EventStore<T> _eventStore;
        private readonly string _consumerGroup;
        private readonly string _stream;
        private readonly IDatabase _db;
        private readonly int _batchSize;

        private string _lastId = "0-0";
        private bool _checkForPending = true;

        public ConsumerGroupReader(
            EventStore<T> eventStore,
            string consumerGroup,
            IDatabase db,
            int batchSize,
            string stream)
        {
            _eventStore = eventStore;
            _consumerGroup = consumerGroup;
            _db = db;
            _batchSize = batchSize;
            _stream = stream;

            CreateConsumerGroupAsync(consumerGroup, stream).GetAwaiter().GetResult();
        }

        public async Task ReadNextBatchAsync(Action<EventData<T>> eventHandler)
        {
            var startFromId = _checkForPending ? _lastId : ">";

            var items = await _db.StreamReadGroupAsync(
                "GROUP",
                _consumerGroup,
                "redis-cqrs-projection-consumer",
                startFromId,
                _batchSize);

            if (items == null) return;

            if (items.Length == 0) _checkForPending = false;

            foreach (var item in items)
            {
                eventHandler.Invoke(await _eventStore.LoadAggregateEventAsync(item));
                
                await _db.StreamAcknowledgeAsync("GROUP", _consumerGroup, item.Id);

                _lastId = item.Id;
            }
        }

        private async Task CreateConsumerGroupAsync(string groupName, string streamName)
        {
            await _db.StreamCreateConsumerGroupAsync(groupName, streamName, 0); // TODO - Is this correct usage
            // TODO - Check err
        }
    }
}