namespace Raft.Infrastructure.Journaler
{
    internal class NoOpEntryPadder : IJournalEntryPadder
    {
        public byte[] AddPaddingToEntry(byte[] entry)
        {
            return entry;
        }
    }
}