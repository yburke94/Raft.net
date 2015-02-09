namespace Raft.Core
{
    public interface IRaftNode
    {
        long CurrentTerm { get; }
        long CommitIndex { get; }

        RaftLog Log { get; }

        void CreateCluster();
        void ScheduleCommandExecution();
        void CommitLogEntry(long entryIdx);
        void ApplyCommand(long entryIdx);
        void SetHigherTerm(long term);
    }
}