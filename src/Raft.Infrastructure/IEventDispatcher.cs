namespace Raft.Infrastructure
{
    /// <summary>
    /// Dispatches events to subscribers subscribed via the <see cref="ISubscribe" /> contract.
    /// </summary>
    public interface IEventDispatcher
    {
        void Publish<TEvent>(TEvent @event);
    }
}
