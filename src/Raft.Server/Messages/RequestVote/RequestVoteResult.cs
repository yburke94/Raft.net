namespace Raft.Server.Messages.RequestVote
{
    public class RequestVoteResult
    {
        /// <summary>
        /// CurrentTerm, for candidate to update itself.
        /// </summary>
        public long Term { get; set; }

        /// <summary>
        /// True means candidate received vote.
        /// </summary>
        public bool VoteGranted { get; set; }
    }
}
