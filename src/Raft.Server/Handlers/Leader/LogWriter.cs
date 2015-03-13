using System;
using Raft.Infrastructure.Journaler;
using Raft.Server.BufferEvents;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder
    ///     LogWriter*
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    public class LogWriter : LeaderEventHandler
    {
        private readonly IJournal _journal;

        public LogWriter(IJournal journal)
        {
            _journal = journal;
        }

        public override void Handle(CommandScheduled @event)
        {
            if (@event.LogEntry == null || @event.EncodedEntry == null)
                throw new InvalidOperationException("Must set EncodedEntry on event before executing this step.");

            _journal.WriteBlock(@event.EncodedEntry);
        }
    }
}
