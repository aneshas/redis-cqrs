using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial.Arenas;

namespace Redis.CQRS.Projections
{
    public class ProjectionAgent<T> where T : class
    {
        public class Configuration
        {
            public int BatchSize { get; private set; } = 500;

            public void WithBatchSize(int batchSize) =>
                BatchSize = batchSize < 1 ? 1 : batchSize;
        }

        private readonly Configuration _configuration = new Configuration();

        private readonly EventStore<T> _eventStore;

        private readonly IProjection[] _projections;

        public ProjectionAgent(EventStore<T> eventStore, IProjection[] projections)
        {
            _eventStore = eventStore;
            _projections = projections;
        }

        public IDisposable Run(Action<Configuration> configure)
        {
            configure.Invoke(_configuration);

            var tasks = _projections.Select(StartSubscription).ToList();

            return Task.WhenAll(tasks);
        }

        private async Task StartSubscription(IProjection projection)
        {
            while (true)
            {
                try
                {
                    await _eventStore.SubscribeToAggregateStreamsAsync(
                        projection.GetType().Name,
                        _configuration.BatchSize,
                        projection.SubscribeToStreams.ToArray(),
                        e =>
                        {
                            // Call projection event handlers
                        });
                }
                catch (Exception e)
                {
                    try
                    {
                        Console.WriteLine(e); // TODO - log to ex logger
                    }
                    catch
                    {
                        // ignored ?
                    }
                }
            }
        }
    }
}