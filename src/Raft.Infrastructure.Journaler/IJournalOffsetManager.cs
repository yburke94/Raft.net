namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalOffsetManager
    {

        int CurrentJournalIndex { get; }

        /// <summary>
        /// Takes into account metadata added by <see cref="JournalFileWriter"/>.
        /// </summary>
        long NextJournalEntryOffset { get; }

        void IncrementJournalIndex();

        void UpdateJournalOffset(int entrySize);
    }
}