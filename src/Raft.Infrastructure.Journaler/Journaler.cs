using System.IO;

namespace Raft.Infrastructure.Journaler
{
    class Journaler : IJournaler
    {
        private readonly JournalConfiguration _journalConfiguration;
        private readonly IJournalFileWriter _journalFileWriter;
        private readonly IJournalMetadata _journalMetadata;
        private readonly IJournalEntryPadder _entryPadder;

        public Journaler(JournalConfiguration journalConfiguration, IJournalFileWriter journalFileWriter, IJournalMetadata journalMetadata, IJournalEntryPadder entryPadder)
        {
            _journalConfiguration = journalConfiguration;
            _journalFileWriter = journalFileWriter;
            _journalMetadata = journalMetadata;
            _entryPadder = entryPadder;

            _journalFileWriter.SetJournal(_journalMetadata.CurrentJournalIndex, _journalMetadata.NextJournalEntryOffset);
        }

        public void WriteBlock(byte[] block)
        {
            WriteBlockWithoutFlush(block);

            _journalMetadata.Flush();
            _journalFileWriter.Flush();
        }

        public void WriteBlocks(byte[][] blocks)
        {
            foreach (var block in blocks)
            {
                WriteBlockWithoutFlush(block);
            }

            _journalMetadata.Flush();
            _journalFileWriter.Flush();
        }

        private void WriteBlockWithoutFlush(byte[] blockToWrite)
        {
            var paddedEntry = _entryPadder.AddPaddingToEntry(blockToWrite);

            if ((_journalMetadata.NextJournalEntryOffset + paddedEntry.Length) > _journalConfiguration.LengthInBytes)
            {
                _journalMetadata.IncrementJournalIndex();
                _journalFileWriter.SetJournal(_journalMetadata.CurrentJournalIndex);
            }

            _journalMetadata.RegisterLogEntry(blockToWrite.Length, paddedEntry.Length - blockToWrite.Length);
            _journalFileWriter.WriteBytes(paddedEntry);
        }
    }
}
