using System.Threading.Tasks;

namespace Redis.CQRS.Projections
{
    public interface IHandleEvent<in T> where T : class
    {
        Task HandleAsync(T @event, EventInfo eventInfo);
    }
}
