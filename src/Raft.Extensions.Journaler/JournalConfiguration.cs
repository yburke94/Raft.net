namespace Raft.Extensions.Journaler
{
    public sealed class JournalConfiguration
    {
        public string JournalDirectory { get; private set; }

        public string JournalFileName { get; private set; }

        public long LengthInBytes { get; private set; }

        public IoType IoType { get; private set; }

        public JournalConfiguration(string journalDirectory, string journalFileName, long lengthInBytes, IoType ioType)
        {
            JournalDirectory = journalDirectory;
            JournalFileName = journalFileName;
            LengthInBytes = lengthInBytes;
            IoType = ioType;
        }
    }
}