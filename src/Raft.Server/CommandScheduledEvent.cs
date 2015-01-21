using System;
using System.Threading.Tasks;
using Raft.Server.Commands;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        public TaskCompletionSource<CommandExecutionResult> TaskCompletionSource { get; set; }

        public IRaftCommand Command { get; set; }

        public Guid Id { get; set; }

        public CommandScheduledEvent ResetEvent(IRaftCommand command, TaskCompletionSource<CommandExecutionResult> taskCompletionSource)
        {
            if (TaskCompletionSource != null && IsCompletedSuccessfully())
                throw new InvalidOperationException("The event has not finished processing.");

            if (command == null)
                throw new ArgumentNullException("command");

            if (taskCompletionSource == null)
                throw new ArgumentNullException("taskCompletionSource");

            Command = command;
            TaskCompletionSource = taskCompletionSource;

            Id = Guid.NewGuid();

            return this;
        }

        public bool IsCompletedSuccessfully()
        {
            return TaskCompletionSource.Task.IsCompleted && !IsFaulted();
        }

        public bool IsFaulted()
        {
            return TaskCompletionSource.Task.IsFaulted;
        }
    }
}
