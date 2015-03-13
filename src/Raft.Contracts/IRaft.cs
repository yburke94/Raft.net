using System.Threading.Tasks;
using Raft.Server;
using Raft.Server.Data;

namespace Raft.Contracts
{
    public interface IRaft
    {
        Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new();
    }
}