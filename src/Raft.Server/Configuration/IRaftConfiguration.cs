namespace Raft.Server.Configuration
{
    public interface IRaftConfiguration {
        string LogDirectory { get; set; }
        string JournalFileName { get; set; }
        long JournalFileLength { get; set; }
    }
}