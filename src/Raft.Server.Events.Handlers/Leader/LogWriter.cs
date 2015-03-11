using System;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Journaler;

namespace Raft.Server.Events.Handlers.Leader
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter*
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    public class LogWriter : LeaderEventHandler, ISkipInternalCommands
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
                throw new InvalidOperationException("Must set EncodedEntry on event before executing this step.");

            _journal.WriteBlock(@event.EncodedEntry);
        }
    }
}
