using System;
using System.Threading.Tasks;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents
{
    internal class InternalCommandScheduled : BufferEvent, IEventTranslator<InternalCommandScheduled>
    {
        public INodeCommand Command { get; set; }

        public InternalCommandScheduled Translate(InternalCommandScheduled existingEvent, long sequence)
        {
            if (Command == null)
                throw new InvalidOperationException("NodeCommand must be set when translating existing event.");

            existingEvent.Command = Command;
            existingEvent.CompletionSource = new TaskCompletionSource<object>();
            return existingEvent;
        }
    }
}