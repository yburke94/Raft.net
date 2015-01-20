namespace Raft.Infrastructure.Journaler.Transformers
{
    internal interface ITransformJournalEntry
    {
        byte[] Transform(byte[] block);
    }
}
