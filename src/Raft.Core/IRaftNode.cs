namespace Raft.Core
{
    public interface IRaftNode
    {
        long CurrentTerm { get; }
        long LastLogIndex { get; }

        long?[] Log { get; }

        void JoinCluster();
        void LogEntry();
        void EntryLogged();
    }
}