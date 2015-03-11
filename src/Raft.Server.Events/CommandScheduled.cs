using System;
using System.Threading.Tasks;
using Raft.Server.Commands;
using Raft.Server.Events.Data;

namespace Raft.Server.Events
{
    public class CommandScheduled
    {
        public Guid Id { get; internal set; }

        public IRaftCommand Command { get; internal set; }

        public TaskCompletionSource<CommandExecuted> TaskCompletionSource { get; internal set; }

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
