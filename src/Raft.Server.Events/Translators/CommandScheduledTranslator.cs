using System;
using System.Threading.Tasks;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Commands;
using Raft.Server.Events.Data;

namespace Raft.Server.Events.Translators
{
    public class CommandScheduledTranslator : ITranslator<CommandScheduled>
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly IRaftCommand _command;
        private readonly TaskCompletionSource<CommandExecuted> _taskCompletionSource;

        public CommandScheduledTranslator(IRaftCommand command, TaskCompletionSource<CommandExecuted> taskCompletionSource)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (taskCompletionSource == null)
                throw new ArgumentNullException("taskCompletionSource");

            _command = command;
            _taskCompletionSource = taskCompletionSource;
        }

        public CommandScheduled Translate(CommandScheduled existingEvent, long sequence)
        {
            existingEvent.Id = _id;
            existingEvent.Command = _command;
            existingEvent.TaskCompletionSource = _taskCompletionSource;
            existingEvent.LogEntry = null;
            existingEvent.EncodedEntry = null;
            return existingEvent;
        }
    }
}