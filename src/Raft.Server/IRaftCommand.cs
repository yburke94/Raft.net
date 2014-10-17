namespace Raft.Server
{
    public interface IRaftCommand
    {
        string CommandName { get; }

        void Execute(RaftServerContext context);
    }
}