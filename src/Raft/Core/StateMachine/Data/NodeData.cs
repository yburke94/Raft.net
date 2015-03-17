using System;

namespace Raft.Core.StateMachine.Data
{
    internal class NodeData
    {
        public NodeData()
        {
            NodeId = Guid.NewGuid();
            Log = new NodeLog();

            CurrentTerm = 0;
            VotedFor = null;

            CommitIndex = 0;
            LastApplied = 0;
        }

        /// <summary>
        /// Id for the Node.
        /// </summary>
        /// <remarks>
        /// Assigned at startup.
        /// </remarks>
        public Guid NodeId { get; set; }

        /// <summary>
        /// Latest term server has seen.
        /// </summary>
        /// <remarks>
        /// Persistant state;
        /// Initialized to 0 on first boot, increases monotonically.
        /// </remarks>
        public long CurrentTerm { get; set; }

        /// <summary>
        /// CandidateId that received vote in current term (or null if none).
        /// </summary>
        /// <remarks>
        /// Persistant state.
        /// </remarks>
        public Guid? VotedFor { get; set; }

        /// <summary>
        /// Index of highest log entry known to be committed.
        /// A log entry is committed once the leader that created the entry has replicated it on a majority of the servers.
        /// </summary>
        /// <remarks>
        /// Volatile state.
        /// Initialized to 0, increases monotonically.
        /// </remarks>
        public long CommitIndex { get; set; }

        /// <summary>
        /// Index of highest log entry applied to state machine.
        /// </summary>
        /// <remarks>
        /// Volatile state.
        /// Initialized to 0, increases monotonically.
        /// </remarks>
        public long LastApplied { get; set; }

        /// <summary>
        /// In memory representation of the committed log.
        /// </summary>
        /// <remarks>
        /// The log itself is persisted via the Journaler.
        /// This in-memory representation is constructed on startup by reading the journals.
        /// </remarks>
        public NodeLog Log { get; set; }
    }
}
