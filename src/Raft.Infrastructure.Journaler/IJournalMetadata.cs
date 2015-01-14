namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalMetadata
    {
        int CurrentJournalIndex { get; }
        long NextJournalEntryOffset { get; }

        void IncrementJournalIndex();

        void RegisterLogEntry(long logLength, long? padding);

        void Flush();
    }
}