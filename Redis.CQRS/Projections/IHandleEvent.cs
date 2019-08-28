using System.Threading.Tasks;

namespace Redis.CQRS.Projections
{
    public interface IHandleEvent<T> where T : class
    {
        Task HandleAsync(EventData<T> eventData);
    }
}
