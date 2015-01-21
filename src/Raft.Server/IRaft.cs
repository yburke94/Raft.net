using System.Threading.Tasks;
using Raft.Server.Commands;

namespace Raft.Server
{
    public interface IRaft
    {
        RaftServerContext Context { get; }

        Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new();
    }
}