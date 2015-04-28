using System;
using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Data;

namespace Raft.Server.BufferEvents
{
    internal class CommandScheduled : BufferEvent, IEventTranslator<CommandScheduled>
    {
        public Guid Id { get; private set; }

        public IRaftCommand Command { get; set; }

        public LogEntry LogEntry { get; private set; }
        public byte[] EncodedEntry { get; private set; }

        public void SetLogEntry(LogEntry entry, byte[] encodedEntry)
        {
            LogEntry = entry;
            EncodedEntry = encodedEntry;
        }

        public CommandScheduled Translate(CommandScheduled existingEvent, long sequence)
        {
            if (Command == null)
                throw new InvalidOperationException("RaftCommand must be set when translating existing event.");

            existingEvent.Id = Guid.NewGuid();
            existingEvent.Command = Command;
            existingEvent.CompletionSource = new TaskCompletionSource<object>();
            existingEvent.LogEntry = null;
            existingEvent.EncodedEntry = null;
            return existingEvent;
        }
    }
}
