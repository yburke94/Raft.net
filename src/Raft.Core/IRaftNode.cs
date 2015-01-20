namespace Raft.Core
{
    public interface IRaftNode
    {
        long CurrentTerm { get; }
        long LastLogIndex { get; }

        long?[] Log { get; }

        void CreateCluster();
        void ExecuteCommand();
        void AddLogEntry();
    }
}