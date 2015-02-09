using System;
using System.Threading.Tasks;
using Raft.Server.Commands;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        public Guid Id { get; set; }

        public IRaftCommand Command { get; set; }

        public TaskCompletionSource<CommandExecutionResult> TaskCompletionSource { get; set; }

        public CommandScheduledEvent ResetEvent(IRaftCommand command, TaskCompletionSource<CommandExecutionResult> taskCompletionSource)
        {
            if (TaskCompletionSource != null && IsCompletedSuccessfully())
                throw new InvalidOperationException("The event has not finished processing.");

            if (command == null)
                throw new ArgumentNullException("command");

            if (taskCompletionSource == null)
                throw new ArgumentNullException("taskCompletionSource");

            Id = Guid.NewGuid();
            Command = command;
            TaskCompletionSource = taskCompletionSource;

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
