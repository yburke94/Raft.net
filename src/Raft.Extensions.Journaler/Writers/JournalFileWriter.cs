using System;
using System.IO;

namespace Raft.Extensions.Journaler.Writers
{
    internal abstract class JournalFileWriter : IJournalFileWriter
    {
        private readonly JournalConfiguration _journalConfiguration;
        protected FileStream CurrentStream;

        protected JournalFileWriter(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

        protected abstract void SetFileStream(string path, bool newFile, long fileSizeInBytes, long startingPosition);

        public void SetJournal(int journalIdx, long startingPosition)
        {
            CloseCurrentStream();

            var journalFileName = _journalConfiguration.JournalFileName + "." + journalIdx;
            var journalPath = Path.Combine(_journalConfiguration.JournalDirectory, journalFileName);

            var newJournal = !File.Exists(journalPath);
            if (newJournal && startingPosition != 0)
                throw new InvalidOperationException("Starting position should be set to 0 when creating a new journal file.");
        }

        public void WriteJournalEntry(byte[] bytes)
        {
            AssertStreamIsSet();
            CurrentStream.Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            AssertStreamIsSet();

            CurrentStream.Flush(true);
        }

        private void AssertStreamIsSet()
        {
            if (CurrentStream == null)
                throw new InvalidOperationException(
                    "Cannot perform this operation as the current file is not set. " +
                    "Please ensure you have made a call to SetJournal prior to writing or flushing the journal file.");
        }

        private void CloseCurrentStream()
        {
            if (CurrentStream == null) return;

            CurrentStream.Dispose();
            CurrentStream = null;
        }

        public void Dispose()
        {
            CloseCurrentStream();
        }
    }
}
