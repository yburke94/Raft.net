using System.Threading.Tasks;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents
{
    internal class NodeCommandScheduled : IFutureEvent<NodeCommandResult>
    {
        public INodeCommand Command { get; set; }

        public TaskCompletionSource<NodeCommandResult> CompletionSource { get; internal set; }
    }
}