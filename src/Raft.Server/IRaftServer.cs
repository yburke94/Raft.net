using Raft.Infrastructure;

namespace Raft.Server
{
    public interface IRaftServer
    {
        IFuture<LogResult> Execute<T>(T command) where T : IRaftCommand, new();
    }
}