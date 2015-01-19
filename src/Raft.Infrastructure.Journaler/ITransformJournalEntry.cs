namespace Raft.Infrastructure.Journaler
{
    internal interface ITransformJournalEntry
    {
        byte[] Transform(byte[] block);
    }
}
