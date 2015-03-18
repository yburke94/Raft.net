using System;
using System.Threading.Tasks;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents
{
    internal class NodeCommandScheduled : IFutureEvent<NodeCommandResult>, IEventTranslator<NodeCommandScheduled>
    {
        public INodeCommand Command { get; set; }

        public TaskCompletionSource<NodeCommandResult> CompletionSource { get; private set; }

        public NodeCommandScheduled Translate(NodeCommandScheduled existingEvent, long sequence)
        {
            if (Command == null)
                throw new InvalidOperationException("NodeCommand must be set when translating existing event.");

            existingEvent.Command = Command;
            existingEvent.CompletionSource = new TaskCompletionSource<NodeCommandResult>();
            return existingEvent;
        }
    }
}