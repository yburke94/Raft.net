using System;
using Disruptor;
using Raft.Server.Commands;
using Raft.Server.Handlers.Contracts;

namespace Raft.Server.Handlers
{
    internal abstract class RaftEventHandler : IEventHandler<CommandScheduledEvent>
    {
        protected long Sequence = 0;
        protected bool EndOfBatch = false;

        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            if (data.IsFaulted() && !(this is IHandleFaultedCommands))
                return;

            if (data.IsCompletedSuccessfully())
                return;

            if (this is ISkipInternalCommands && (data.Command is IRaftInternalCommand))
                return;

            try
            {
                Sequence = sequence;
                EndOfBatch = endOfBatch;
                Handle(data);
            }
            catch (Exception exc)
            {
                data.TaskCompletionSource.SetException(exc);
            }
        }

        public abstract void Handle(CommandScheduledEvent @event);
    }
}
