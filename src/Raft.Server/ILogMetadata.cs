namespace Raft.Server
{
    internal interface ILogMetadata
    {
        long CurrentJournalIndex { get; }

        long NextJournalEntryOffset { get; }

        void IncrementJournalIndex();

        void AddLogEntryToIndex(long logEntryIdx, long dataLength);
    }
}