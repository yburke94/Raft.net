using System.Threading.Tasks;
using Raft.Server.Events;

namespace Raft.Server
{
    public interface IRaft
    {
        Task<CommandExecuted> ExecuteCommand<T>(T command) where T : IRaftCommand, new();
    }
}