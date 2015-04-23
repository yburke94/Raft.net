namespace Raft.Infrastructure
{
    /// <summary>
    /// Subscribe to an event of type <see cref="TEvent"/>>.
    /// </summary>
    internal interface IHandle<in TEvent>
    {
        void Handle(TEvent @event);
    }
}
