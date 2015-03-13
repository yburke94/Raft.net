using System;
using System.Threading.Tasks;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents
{
    public class CommandScheduled
    {
        public Guid Id { get; internal set; }

        public IRaftCommand Command { get; internal set; }

        public TaskCompletionSource<CommandExecutionResult> TaskCompletionSource { get; internal set; }

        public LogEntry LogEntry { get; internal set; }
        public byte[] EncodedEntry { get; internal set; }

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
    }
}
