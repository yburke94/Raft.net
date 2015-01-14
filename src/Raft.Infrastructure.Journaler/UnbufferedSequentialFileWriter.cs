using System;
using System.IO;

namespace Raft.Infrastructure.Journaler
{
    internal class UnbufferedSequentialFileWriter : IJournalFileWriter, IDisposable
    {
        private readonly JournalConfiguration _journalConfiguration;
        private FileStream _currentFile;

        public UnbufferedSequentialFileWriter(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

        public void SetJournal(int journalIdx, long startingPosition)
        {
            CloseCurrentStream();

            var bufferSize = SectorSize.Get(_journalConfiguration.JournalDirectory);
            var journalFileName = _journalConfiguration.JournalFileName + "." + journalIdx;
            var journalPath = Path.Combine(_journalConfiguration.JournalDirectory, journalFileName);

            var newJournal = !File.Exists(journalPath);
            if (newJournal && startingPosition != 0)
                throw new InvalidOperationException("Starting position should be set to 0 when creating a new journal file.");

            _currentFile = UnbufferedStream.Get(journalPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, (int) bufferSize);
            _currentFile.Seek(startingPosition, SeekOrigin.Current);

            if (!newJournal) return;

            _currentFile.SetLength(_journalConfiguration.LengthInBytes);
            Flush();
        }

        public void WriteBytes(byte[] bytes)
        {
            AssertFileIsSet();

            _currentFile.Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            AssertFileIsSet();

            // NoOp if position is not 0
            if (_currentFile.Position != 0) return;

            // Flush NTFS Metadata
            _currentFile.FlushProperly();
        }

        private void AssertFileIsSet()
        {
            if (_currentFile == null)
                throw new InvalidOperationException(
                    "Cannot perform this operation as the current file is not set. " +
                    "Please ensure you have made a call to SetJournal prior to writing or flushing the journal file.");
        }

        private void CloseCurrentStream()
        {
            if (_currentFile == null) return;

            _currentFile.Dispose();
            _currentFile = null;
        }

        public void Dispose()
        {
            CloseCurrentStream();
        }
    }
}
