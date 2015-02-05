using System.Collections.Generic;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;
using Raft.Server.Messages.AppendEntries;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 3 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator*
    ///     CommandApplier
    /// </summary>
    internal class LogReplicator : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IList<PeerNode> _peers;
        private readonly LogEntryRegister _logEntryRegister;

        public LogReplicator(IList<PeerNode> peers, LogEntryRegister logEntryRegister)
        {
            _peers = peers;
            _logEntryRegister = logEntryRegister;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var encodedEntry = _logEntryRegister.GetEncodedLog(@event.Id);

            var request = new AppendEntriesRequest {
                Entries = new[] {
                    encodedEntry.Value
                }
            };

            foreach (var peerNode in _peers)
            {
                peerNode.Channel.AppendEntries(request);
            }
        }

    }
}
