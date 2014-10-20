namespace Raft.Server
{
    public interface IRaftCommand
    {
        void Execute(RaftServerContext context);
    }
}