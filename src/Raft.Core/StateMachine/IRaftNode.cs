using System;
using Raft.Core.Log;
using Raft.Core.StateMachine.Enums;

namespace Raft.Core.StateMachine
{
    public interface IRaftNode
    {
        NodeState CurrentState { get; }

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
        /// A log entry is committed once the leader that created the entry has replicated it on a majority of the servers.
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
        NodeLog Log { get; }

        // Initialization Actions
        void CreateCluster();
        void JoinCluster();

        // Log Actions
        void ScheduleCommandExecution();
        void CommitLogEntry(long entryIdx, long term);
        void ApplyCommand(long entryIdx);

        // Cluster Actions
        void SetTermFromRpc(long term);
        void TimeoutLeaderHeartbeat();
        void WinCandidateElection();
    }
}