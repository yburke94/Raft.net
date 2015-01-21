namespace Raft.Core
{
    public interface IRaftNode
    {
        long CurrentTerm { get; }
        long CommitIndex { get; }

        RaftLog Log { get; }

        void CreateCluster();
        void ScheduleCommandExecution();
        void AddLogEntry();
        void ApplyCommand();
    }
}