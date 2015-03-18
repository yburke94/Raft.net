namespace Raft.Service.Contracts
{
    public class RequestVoteResponse
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
