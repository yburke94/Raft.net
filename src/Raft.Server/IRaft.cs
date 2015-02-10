using System.Threading.Tasks;
using Raft.Server.Commands;
using Raft.Server.Events;

namespace Raft.Server
{
    public interface IRaft
    {
        RaftServerContext Context { get; }

        Task<CommandExecuted> ExecuteCommand<T>(T command) where T : IRaftCommand, new();
    }
}