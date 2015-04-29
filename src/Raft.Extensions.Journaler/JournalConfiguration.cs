namespace Raft.Extensions.Journaler
{
    public sealed class JournalConfiguration
    {
        private const string DefaultJournalFileName = "journal.data";

        public string JournalDirectory { get; private set; }

        public string JournalFileName { get; private set; }

        public long LengthInBytes { get; private set; }

        public IoType IoType { get; private set; }

        public bool ValidateJournalSequenceOnRead { get; set; }

        public JournalConfiguration(string journalDirectory)
            :this (journalDirectory, DefaultJournalFileName) { }

        public JournalConfiguration(string journalDirectory, string journalFileName)
            : this(journalDirectory, journalFileName, 1024*1024*100, IoType.Unbuffered) {}

        public JournalConfiguration(string journalDirectory, string journalFileName, long lengthInBytes, IoType ioType)
            : this(journalDirectory, journalFileName, lengthInBytes, ioType, true) { }

        public JournalConfiguration(string journalDirectory, string journalFileName,
            long lengthInBytes, IoType ioType, bool validateJournalSequenceOnRead)
        {
            JournalDirectory = journalDirectory;
            JournalFileName = journalFileName;
            LengthInBytes = lengthInBytes;
            IoType = ioType;
            ValidateJournalSequenceOnRead = validateJournalSequenceOnRead;
        }
    }
}