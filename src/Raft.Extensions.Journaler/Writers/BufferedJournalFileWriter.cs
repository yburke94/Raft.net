
using System.IO;

namespace Raft.Extensions.Journaler.Writers
{
    internal sealed class BufferedJournalFileWriter : JournalFileWriter
    {
        public BufferedJournalFileWriter(JournalConfiguration journalConfiguration)
            : base(journalConfiguration) { }

        protected override void SetFileStream(string path, bool newFile, long fileSizeInBytes, long startingPosition)
        {
            const int bufferSize = 2 << 11;

            CurrentStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.None, bufferSize, FileOptions.SequentialScan);

            CurrentStream.Seek(startingPosition, SeekOrigin.Current);

            if (!newFile) return;

            CurrentStream.SetLength(fileSizeInBytes);
            Flush();
        }
    }
}
