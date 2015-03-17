using System;
using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents.Translators
{
    public class CommandScheduledTranslator : ITranslator<CommandScheduled>
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly IRaftCommand _command;
        private readonly TaskCompletionSource<CommandExecutionResult> _taskCompletionSource;

        public CommandScheduledTranslator(IRaftCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            _command = command;
            _taskCompletionSource = new TaskCompletionSource<CommandExecutionResult>();
        }

        public CommandScheduled Translate(CommandScheduled existingEvent, long sequence)
        {
            existingEvent.Id = _id;
            existingEvent.Command = _command;
            existingEvent.CompletionSource = _taskCompletionSource;
            existingEvent.LogEntry = null;
            existingEvent.EncodedEntry = null;
            return existingEvent;
        }
    }
}