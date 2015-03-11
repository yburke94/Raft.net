namespace Raft.Server.Events
{
    public class CommitCommandRequested
    {
        public byte[] Entry { get; internal set; }
    }
}