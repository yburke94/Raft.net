namespace Raft.Server.Commands
{
    public interface IRaftCommand
    {
        void Execute(RaftServerContext context);
    }
}