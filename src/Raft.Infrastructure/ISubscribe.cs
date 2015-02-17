namespace Raft.Infrastructure
{
    /// <summary>
    /// Subscribe to events dispatched via the  <see cref="IEventDispatcher" /> dispatcher.
    /// </summary>
    public interface ISubscribe<in TEvent>
    {
        void Handle(TEvent @event);
    }
}
