namespace Raft.Infrastructure
{
    /// <summary>
    /// Dispatches events to subscribers subscribed via the <see cref="IHandle{TEvent}" /> contract.
    /// </summary>
    public interface IEventDispatcher
    {
        void Publish<TEvent>(TEvent @event);
    }
}
