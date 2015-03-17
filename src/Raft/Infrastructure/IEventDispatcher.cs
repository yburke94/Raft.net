namespace Raft.Infrastructure
{
    /// <summary>
    /// Dispatches events to subscribers subscribed via the <see cref="IHandle{TEvent}" /> contract.
    /// </summary>
    internal interface IEventDispatcher
    {
        void Publish<TEvent>(TEvent @event);
    }
}
