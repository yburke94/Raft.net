using System;

namespace Raft.Service.Contracts
{
    /// <summary>
    /// Invoked by leader to replicate log entries; also used as heartbeat.
    /// </summary>
    /// <remarks>
    /// 1. Reply false if term &lt; currentTerm.
    /// 2. Reply false if log doesn’t contain an entry at prevLogIndex whose term matches prevLogTerm.
    /// 3. If an existing entry conflicts with a new one (same index but different terms), delete the existing entry and all that follow it.
    /// 4. Append any new entries not already in the log.
    /// 5. If leaderCommit &gt; commitIndex, set commitIndex = min(leaderCommit, index of last new entry).
    /// </remarks>
    public class AppendEntriesRequest
    {
        /// <summary>
        /// Leader’s term.
        /// </summary>
        public long Term { get; set; }

        /// <summary>
        /// So follower can redirect clients.
        /// </summary>
        public Guid LeaderId { get; set; }

        /// <summary>
        /// Index of log entry immediately preceding new ones.
        /// </summary>
        public long PreviousLogIndex { get; set; }

        /// <summary>
        /// Term of PreviousLogIndex entry.
        /// </summary>
        public long PreviousLogTerm { get; set; }

        /// <summary>
        /// Log entries to store (empty for heartbeat; may send more than one for efficiency).
        /// </summary>
        /// <remarks>
        /// Log entries are sent already encoded via proto-buf.
        /// </remarks>
        public byte[,] Entries { get; set; }

        /// <summary>
        /// Leader’s commitIndex.
        /// </summary>
        public long LeaderCommit { get; set; }
    }
}
