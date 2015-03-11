using System;
using Disruptor;
using Raft.Server.Commands.Internal;

namespace Raft.Server.Events.Handlers.Leader
{
    public abstract class LeaderEventHandler : IEventHandler<CommandScheduled>
    {
        protected long Sequence = 0;
        protected bool EndOfBatch = false;

        public void OnNext(CommandScheduled data, long sequence, bool endOfBatch)
        {
            if (data.IsFaulted())
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

        public abstract void Handle(CommandScheduled @event);
    }
}
