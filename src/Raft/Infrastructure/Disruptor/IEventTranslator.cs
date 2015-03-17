namespace Raft.Infrastructure.Disruptor
{
    internal interface IEventTranslator<TEvent>
    {
        TEvent Translate(TEvent existingEvent, long sequence);
    }
}
