namespace Raft.Infrastructure
{
    /// <summary>
    /// Subscribe to events dispatched via the  <see cref="IEventDispatcher" /> dispatcher.
    /// </summary>
    internal interface IHandle<in TEvent>
    {
        void Handle(TEvent @event);
    }
}
