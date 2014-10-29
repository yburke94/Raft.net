using System.Threading.Tasks;

namespace Raft.Server
{
    public interface IRaftServer
    {
        Task<LogResult> Execute<T>(T command) where T : IRaftCommand, new();
    }
}