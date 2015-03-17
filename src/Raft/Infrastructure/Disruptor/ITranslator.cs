namespace Raft.Infrastructure.Disruptor
{
    public interface ITranslator<TEvent>
    {
        TEvent Translate(TEvent existingEvent, long sequence);
    }
}
