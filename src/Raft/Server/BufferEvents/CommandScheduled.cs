using System;
using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents
{
    public class CommandScheduled : IFutureEvent<CommandExecutionResult>
    {
        public Guid Id { get; internal set; }

        public IRaftCommand Command { get; internal set; }

        public LogEntry LogEntry { get; internal set; }
        public byte[] EncodedEntry { get; internal set; }

        public TaskCompletionSource<CommandExecutionResult> CompletionSource { get; internal set; }

        public bool IsCompletedSuccessfully()
        {
            return CompletionSource.Task.IsCompleted && !IsFaulted();
        }

        public bool IsFaulted()
        {
            return CompletionSource.Task.IsFaulted;
        }

        public void SetLogEntry(LogEntry entry, byte[] encodedEntry)
        {
            LogEntry = entry;
            EncodedEntry = encodedEntry;
        }
    }
}
