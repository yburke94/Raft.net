using System;
using System.Threading.Tasks;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        public TaskCompletionSource<LogResult> TaskCompletionSource { get; set; }

        public IRaftCommand Command { get; set; }

        public Guid Id { get; set; }

        public CommandScheduledEvent ResetEvent(IRaftCommand command, TaskCompletionSource<LogResult> taskCompletionSource)
        {
            if (TaskCompletionSource != null && IsValidForProcessing())
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

        public bool IsValidForProcessing()
        {
            return !TaskCompletionSource.Task.IsCompleted;
        }
    }
}
