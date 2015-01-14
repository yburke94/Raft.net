namespace Raft.Infrastructure.Journaler
{
    internal sealed class JournalConfiguration
    {
        public string JournalDirectory { get; set; }

        public string JournalFileName { get; set; }

        public long LengthInBytes { get; set; }
    }
}