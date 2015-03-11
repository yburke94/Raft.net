using System.Threading.Tasks;
using Raft.Server.Commands;
using Raft.Server.Events.Data;

namespace Raft.Contracts
{
    public interface IRaft
    {
        Task<CommandExecuted> ExecuteCommand<T>(T command) where T : IRaftCommand, new();
    }
}