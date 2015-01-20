using System.IO;
using Raft.Infrastructure.Journaler.KernelHelpers;

namespace Raft.Infrastructure.Journaler.Writers
{
    internal sealed class UnbufferedJournalFileWriter : JournalFileWriter
    {
        public UnbufferedJournalFileWriter(JournalConfiguration journalConfiguration)
            : base(journalConfiguration) { }

        protected override void SetFileStream(string path, bool newFile, long fileSizeInBytes, long startingPosition)
        {
            const int bufferSize = 2 << 11;

            CurrentStream = UnbufferedStream.Get(
                path, FileMode.OpenOrCreate,
                FileAccess.Write, FileShare.None, bufferSize);

            CurrentStream.Seek(startingPosition, SeekOrigin.Current);

            if (!newFile) return;

            CurrentStream.SetLength(fileSizeInBytes);
            Flush();
        }
    }
}
