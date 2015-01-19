using System;

namespace Raft.Infrastructure.Journaler
{
    internal class JournalOffsetManager
    {
        private readonly long _journalLengthInBytes;

        public JournalOffsetManager(JournalConfiguration journalConfiguration)
        {
            _journalLengthInBytes = journalConfiguration.LengthInBytes;

            CurrentJournalIndex = 0;
            NextJournalEntryOffset = 0;
        }

        public int CurrentJournalIndex { get; private set; }

        public long NextJournalEntryOffset { get; private set; }

        public void IncrementJournalIndex()
        {
            CurrentJournalIndex++;
            NextJournalEntryOffset = 0;
        }

        public void UpdateJournalOffset(int entryLength)
        {
            if ((NextJournalEntryOffset + entryLength) > _journalLengthInBytes)
                throw new InvalidOperationException(
                    "This operation would exceed the length of the journal file. " +
                    "You must increment the journal index in order to write this journal entry.");

            NextJournalEntryOffset += entryLength;
        }
    }
}
