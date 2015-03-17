using System.Threading.Tasks;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents.Translators
{
    internal class NodeCommandTranslator : IEventTranslator<NodeCommandScheduled>
    {
        private readonly INodeCommand _nodeCommand;
        private readonly TaskCompletionSource<NodeCommandResult> _taskCompletionSource;

        public NodeCommandTranslator(INodeCommand nodeCommand)
        {
            _nodeCommand = nodeCommand;
            _taskCompletionSource = new TaskCompletionSource<NodeCommandResult>();
        }

        public NodeCommandScheduled Translate(NodeCommandScheduled existingEvent, long sequence)
        {
            existingEvent.Command = _nodeCommand;
            existingEvent.CompletionSource = _taskCompletionSource;
            return existingEvent;
        }
    }
}