using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redis.CQRS.Projections;
using StackExchange.Redis;
using Tactical.DDD;

namespace Redis.CQRS.Tests.Projections
{
    [TestClass]
    public class ProjectionAgentTests
    {
        private readonly EventStore<IDomainEvent> _eventStore;

        public ProjectionAgentTests()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost");
            _eventStore = new EventStore<IDomainEvent>(connectionMultiplexer);

            _eventStore.SaveAsync("User", "user-1234", 0, new IDomainEvent[]
                {
                    new UserRegistered("me@anes.io", "aneshas", 31),
                    new UserRegistered("john@doe.io", "john", 45),
                    new UserRegistered("jane@doe.io", "jane", 22),
                })
                .GetAwaiter()
                .GetResult();
        }

        [TestMethod]
        public async Task Test()
        {
            var projection = new RegisteredUsers();

            var agent = ProjectionAgent.Instance(
                _eventStore,
                new IProjection[]
                {
                    projection
                }
            );

            // TODO - Run should be idempotent
            agent.Run();

            await Task.Delay(3000); // TODO - Is there a cleaner way

            agent.Dispose(); 

            Assert.AreEqual(3, projection.Events.Count()); 
        }
    }
}