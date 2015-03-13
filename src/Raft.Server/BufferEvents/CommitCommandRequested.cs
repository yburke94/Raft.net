namespace Raft.Server.BufferEvents
{
    public class CommitCommandRequested
    {
        public byte[] Entry { get; internal set; }
    }
}