namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalFileWriter
    {
        void SetJournal(int journalIdx, long startingPosition = 0L);

        void WriteJournalEntry(byte[] bytes);

        void Flush();
    }
}