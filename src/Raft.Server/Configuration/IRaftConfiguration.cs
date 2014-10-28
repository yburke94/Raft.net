namespace Raft.Server.Configuration
{
    public interface IRaftConfiguration {
        string LogPath { get; set; }
    }
}