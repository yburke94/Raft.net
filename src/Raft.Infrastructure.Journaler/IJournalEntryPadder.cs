namespace Raft.Infrastructure.Journaler
{
    interface IJournalEntryPadder
    {
        byte[] AddPaddingToEntry(byte[] entry);
    }
}
