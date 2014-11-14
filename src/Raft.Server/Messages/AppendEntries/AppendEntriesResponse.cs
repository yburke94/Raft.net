namespace Raft.Server.Messages.AppendEntries
{
    public class AppendEntriesResponse
    {
        /// <summary>
        /// currentTerm, for leader to update itself.
        /// </summary>
        public long Term { get; set; }

        /// <summary>
        /// true if follower contained entry matching prevLogIndex and prevLogTerm
        /// </summary>
        public bool Success { get; set; }
    }
}