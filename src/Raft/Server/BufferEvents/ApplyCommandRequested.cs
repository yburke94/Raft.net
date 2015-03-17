namespace Raft.Server.BufferEvents
{
    internal class ApplyCommandRequested
    {
        public long LogIdx { get; internal set; }
    }
}