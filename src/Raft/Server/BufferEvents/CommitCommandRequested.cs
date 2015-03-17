namespace Raft.Server.BufferEvents
{
    internal class CommitCommandRequested
    {
        public byte[] Entry { get; internal set; }
    }
}