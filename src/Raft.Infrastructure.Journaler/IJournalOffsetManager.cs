namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalOffsetManager
    {
        int CurrentJournalIndex { get; }

        long NextJournalEntryOffset { get; }

        void IncrementJournalIndex();

        void UpdateJournalOffset(int entrySize);
    }
}