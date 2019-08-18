using System;
using System.Reflection;
using Pipelines.Sockets.Unofficial.Arenas;

namespace Redis.CQRS.Projections
{
    public class ProjectionAgent<T> where T : class
    {
        public class Configuration
        {
            public int BatchSize { get; private set; } = 100;

            public string ConsumerGroupName { get; private set; } 
                = Assembly.GetExecutingAssembly().FullName;

            public void WithBatchSize(int batchSize) =>
                BatchSize = batchSize < 1 ? 1 : batchSize;

            public void WithConsumerGroupName(string name) =>
                ConsumerGroupName = name;
        }

        private readonly EventStore<T> _eventStore;

        private readonly IProjection[] _projections;

        private readonly Configuration _configuration = new Configuration();

        public ProjectionAgent(EventStore<T> eventStore, IProjection[] projections)
        {
            _eventStore = eventStore;
            _projections = projections;
        }

        public IDisposable Run(Action<Configuration> configure)
        {
            configure.Invoke(_configuration);

            // Run one subscription for each projection

            return null;
        }
    }
}