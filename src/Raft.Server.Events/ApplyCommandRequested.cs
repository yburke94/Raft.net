namespace Raft.Server.Events
{
    public class ApplyCommandRequested
    {
        public long LogIdx { get; internal set; }
    }
}