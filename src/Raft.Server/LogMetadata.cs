namespace Raft.Server
{
    public class LogMetadata : ILogMetadata
    {
        public LogMetadata(long journalIdx, long nextOffset)
        {
            CurrentJournalIndex = journalIdx;
            NextJournalEntryOffset = nextOffset;
        }
        public long CurrentJournalIndex { get; private set; }

        public long NextJournalEntryOffset { get; private set; }

        public void IncrementJournalIndex()
        {
            CurrentJournalIndex++;
            NextJournalEntryOffset = 0;
        }

        public void AddLogEntryToIndex(long logEntryIdx, long dataLength)
        {
            NextJournalEntryOffset = NextJournalEntryOffset + dataLength;
        }
    }
}