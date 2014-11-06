using System;

namespace Raft.Server.Messages.RequestVote
{
    /// <summary>
    /// Invoked by candidates to gather votes.
    /// </summary>
    /// <remarks>
    /// RECIEVER IMPLEMENTATION:
    /// 1. Reply false if term &lt; currentTerm.
    /// 2. If votedFor is null or candidateId, and candidate’s log is at least as up-to-date as receiver’s log, grant vote.
    /// </remarks>
    public class RequestVote
    {
        /// <summary>
        /// Candidate’s term.
        /// </summary>
        public long Term { get; set; }

        /// <summary>
        /// candidate requesting vote.
        /// </summary>
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Index of candidate’s last log entry.
        /// </summary>
        public long LastLogIndex { get; set; }

        /// <summary>
        /// Term of candidate’s last log entry.
        /// </summary>
        public long LastLogTerm { get; set; }
    }
}
