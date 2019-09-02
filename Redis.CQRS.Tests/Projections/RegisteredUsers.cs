using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Redis.CQRS.Projections;
using Tactical.DDD;

namespace Redis.CQRS.Tests.Projections
{
    public class RegisteredUsers : 
        IProjection,
        IHandleEvent<UserRegistered>
    {
        public readonly List<IDomainEvent> Events =  new List<IDomainEvent>();
        
        public IEnumerable<string> SubscribeToStreams()
        {
            yield return "User";
        }

        public Task HandleAsync(UserRegistered @event, EventInfo eventInfo)
        {
            Events.Add(@event);
            
            return Task.CompletedTask;
        }
    }
}