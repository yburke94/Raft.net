namespace Raft.Server
{
    internal interface ILogMetadata
    {
        int CurrentJournalIndex { get; }

        long CurrentJournalOffset { get; }

        void IncrementJournalIndex();

        void SetJournalOffset(long offset);
    }
}