using System;
using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    internal abstract class CommandScheduledEventHandler : IEventHandler<CommandScheduledEvent>
    {
        private readonly bool _skipInternalCommands;

        protected CommandScheduledEventHandler(bool skipInternalCommands = false)
        {
            _skipInternalCommands = skipInternalCommands;
        }

        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            if (!data.IsValidForProcessing())
                return;

            if (_skipInternalCommands && (data.Command is IRaftInternalCommand))
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

        public abstract void Handle(CommandScheduledEvent data);
    }
}
