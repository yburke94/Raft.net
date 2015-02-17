using System;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Events;
using Raft.Server.Handlers.Contracts;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter*
    ///     LogReplicator
    ///     CommandApplier
    /// </summary>
    internal class LogWriter : LeaderEventHandler, ISkipInternalCommands
    {
        private readonly IJournal _journal;
        private readonly IRaftNode _raftNode;

        public LogWriter(IJournal journal, IRaftNode raftNode)
        {
            _journal = journal;
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduled @event)
        {
            if (@event.LogEntry == null || @event.EncodedEntry == null)
                throw new InvalidOperationException("Must set LogEntry on event before executing this step.");

            _journal.WriteBlock(@event.EncodedEntry);
            _raftNode.CommitLogEntry(@event.LogEntry.Index);
        }
    }
}
