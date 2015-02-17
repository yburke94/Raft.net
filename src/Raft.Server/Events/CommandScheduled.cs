using System;
using System.Threading.Tasks;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Commands;
using Raft.Server.Log;

namespace Raft.Server.Events
{
    internal class CommandScheduled
    {
        public Guid Id { get; private set; }

        public IRaftCommand Command { get; private set; }

        public TaskCompletionSource<CommandExecuted> TaskCompletionSource { get; private set; }

        public LogEntry LogEntry { get; private set; }
        public byte[] EncodedEntry { get; private set; }

        public bool IsCompletedSuccessfully()
        {
            return TaskCompletionSource.Task.IsCompleted && !IsFaulted();
        }

        public bool IsFaulted()
        {
            return TaskCompletionSource.Task.IsFaulted;
        }

        public void SetLogEntry(LogEntry entry, byte[] encodedEntry)
        {
            LogEntry = entry;
            EncodedEntry = encodedEntry;
        }

        internal class Translator : ITranslator<CommandScheduled>
        {
            private readonly Guid _id = Guid.NewGuid();
            private readonly IRaftCommand _command;
            private readonly TaskCompletionSource<CommandExecuted> _taskCompletionSource;

            public Translator(IRaftCommand command, TaskCompletionSource<CommandExecuted> taskCompletionSource)
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
}
