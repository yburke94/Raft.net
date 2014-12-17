namespace Raft.Core
{
    public interface IRaftNode
    {
        long CurrentLogTerm { get; }
        long LastLogIndex { get; }


        void JoinCluster();
        void LogEntry();
        void EntryLogged();
    }
}