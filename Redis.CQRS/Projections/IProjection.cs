using System.Collections.Generic;

namespace Redis.CQRS.Projections
{
    public interface IProjection
    {
        IEnumerable<string> SubscribeToStreams { get; }

        long InitialEventId { get; }
    }
}