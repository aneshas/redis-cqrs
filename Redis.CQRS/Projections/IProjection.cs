using System.Collections.Generic;

namespace Redis.CQRS.Projections
{
    // Use concrete class name (lower snake case as consumer group name)
    public interface IProjection
    {
        IEnumerable<string> SubscribeToStreams();
    }
}