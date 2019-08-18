namespace Redis.CQRS
{
    public sealed class StreamEntry<T>
    {
        public long Id { get; }

        public T Event { get; }

        public void Ack() { }

        // Implement implicit conversion to T ?? does it make sense?
    }
}