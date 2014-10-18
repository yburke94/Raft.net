namespace Raft.Core
{
    public interface IRaftNode
    {
        void JoinCluster();

        void LogEntry();
    }
}