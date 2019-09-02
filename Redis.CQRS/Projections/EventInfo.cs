namespace Redis.CQRS.Projections
{
    public class EventInfo
    {
        public long Id { get; }
        
        // Meta

        internal EventInfo(long id)
        {
            Id = id;
        }
    }
}