using Raft.Infrastructure;

namespace Raft.Server
{
    public interface IRaftServer
    {
        IFuture<ILogResult> Execute<T>(T command) where T : IRaftCommand;
    }
}