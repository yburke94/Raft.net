namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalFileWriter
    {
        void SetJournal(int journalIdx, long startingPosition = 0L);

        void WriteBytes(byte[] bytes);

        void Flush();
    }
}