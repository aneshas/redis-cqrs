using System.Threading.Tasks;

namespace Redis.CQRS.Projections
{
    public interface IHandleEvent<T>
    {
        Task HandleAsync(EventData<T> eventData);
    }
}
