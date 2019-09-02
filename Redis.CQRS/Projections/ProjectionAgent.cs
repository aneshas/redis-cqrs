using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial.Arenas;

namespace Redis.CQRS.Projections
{
    public static class ProjectionAgent
    {
        public static ProjectionAgent<T> Instance<T>(EventStore<T> eventStore, IProjection[] projections)
            where T : class
        {
            // TODO Implement singleton
            return new ProjectionAgent<T>(eventStore, projections);
        }
        
    }
    
    public class ProjectionAgent<T> : IDisposable where T : class
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
        
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private List<Task> _projectionSubscriptions = new List<Task>();

        internal ProjectionAgent(EventStore<T> eventStore, IProjection[] projections)
        {
            _eventStore = eventStore;
            _projections = projections;
        }

        public IDisposable RunWith(Action<Configuration> configuration)
        {
            configuration.Invoke(_configuration);

            return Run();
        }

        public IDisposable Run()
        {
            _projectionSubscriptions = _projections.Select(StartSubscription).ToList();

            return this;
        }

        private async Task StartSubscription(IProjection projection)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _eventStore.SubscribeToAggregateStreamsAsync(
                        _cancellationTokenSource,
                        projection.GetType().Name,
                        _configuration.BatchSize,
                        projection.SubscribeToStreams().ToArray(),
                        async e =>
                        {
                            var eventInfo = new EventInfo(e.Id);

                            try
                            {
                                await ((dynamic) projection).HandleAsync((dynamic) e.Event, eventInfo);
                            }
                            catch
                            {
                                // ignore missing event handler
                            }
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
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                
                Task.WhenAll(_projectionSubscriptions).Wait();

                _cancellationTokenSource.Dispose();
            }
        }
    }
}