namespace Raft.Infrastructure.Journaler
{
    class Journaler : IJournaler
    {
        private readonly JournalConfiguration _journalConfiguration;
        private readonly IJournalFileWriter _journalFileWriter;
        private readonly IJournalOffsetManager _journalOffsetManager;
        private readonly IJournalEntryPadder _entryPadder;

        public Journaler(JournalConfiguration journalConfiguration, IJournalFileWriter journalFileWriter, IJournalOffsetManager journalOffsetManager, IJournalEntryPadder entryPadder)
        {
            _journalConfiguration = journalConfiguration;
            _journalFileWriter = journalFileWriter;
            _journalOffsetManager = journalOffsetManager;
            _entryPadder = entryPadder;

            _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex, _journalOffsetManager.NextJournalEntryOffset);
        }

        public void WriteBlock(byte[] block)
        {
            WriteBlockWithoutFlush(block);

            _journalFileWriter.Flush();
        }

        public void WriteBlocks(byte[][] blocks)
        {
            foreach (var block in blocks)
            {
                WriteBlockWithoutFlush(block);
            }

            _journalFileWriter.Flush();
        }

        private void WriteBlockWithoutFlush(byte[] blockToWrite)
        {
            var paddedEntry = _entryPadder.AddPaddingToEntry(blockToWrite);

            if ((_journalOffsetManager.NextJournalEntryOffset + paddedEntry.Length) > _journalConfiguration.LengthInBytes)
            {
                _journalOffsetManager.IncrementJournalIndex();
                _journalFileWriter.SetJournal(_journalOffsetManager.CurrentJournalIndex);
            }

            _journalOffsetManager.UpdateJournalOffset(paddedEntry.Length);
            _journalFileWriter.WriteJournalEntry(paddedEntry);
        }
    }
}
