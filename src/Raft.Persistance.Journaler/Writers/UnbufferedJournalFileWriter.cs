using System.IO;
using Raft.Persistance.Journaler.Kernel;

namespace Raft.Persistance.Journaler.Writers
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

        protected override void Write(byte[] bytes)
        {
            CurrentStream.Write(bytes, 0, bytes.Length);
        }
    }
}
