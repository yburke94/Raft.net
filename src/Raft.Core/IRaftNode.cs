using System;

namespace Raft.Core
{
    public interface IRaftNode
    {
        /// <summary>
        /// Id for the Node.
        /// </summary>
        Guid NodeId { get; }

        /// <summary>
        /// Latest term server has seen.
        /// </summary>
        /// <remarks>initialized to 0 on first boot, increases monotonically</remarks>
        long CurrentTerm { get; }

        /// <summary>
        /// Index of highest log entry known to be committed.
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically.</remarks>
        long CommitIndex { get; }

        /// <summary>
        /// Index of highest log entry applied to state machine.
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically</remarks>
        long LastApplied { get; }

        /// <summary>
        /// In memory representation of the committed log.
        /// </summary>
        RaftLog Log { get; }

        void CreateCluster();
        void ScheduleCommandExecution();
        void CommitLogEntry(long entryIdx);
        void ApplyCommand(long entryIdx);
        void SetHigherTerm(long term);
    }
}