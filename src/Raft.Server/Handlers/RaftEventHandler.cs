using System;
using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    internal abstract class RaftEventHandler : IEventHandler<CommandScheduledEvent>
    {
        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            if (!data.IsValidForProcessing())
                return;

            if (this is ISkipInternalCommands && (data.Command is IRaftInternalCommand))
                return;

            try
            {
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
