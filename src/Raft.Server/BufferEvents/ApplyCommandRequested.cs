namespace Raft.Server.BufferEvents
{
    public class ApplyCommandRequested
    {
        public long LogIdx { get; internal set; }
    }
}