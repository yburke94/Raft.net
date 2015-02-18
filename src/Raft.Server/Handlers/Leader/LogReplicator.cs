using System.Collections.Generic;
using Raft.Server.Events;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Messages.AppendEntries;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 3 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator*
    ///     CommandFinalizer
    /// </summary>
    internal class LogReplicator : LeaderEventHandler, ISkipInternalCommands
    {
        private readonly IList<PeerNode> _peers;

        public LogReplicator(IList<PeerNode> peers)
        {
            _peers = peers;
        }

        public override void Handle(CommandScheduled @event)
        {
            var request = new AppendEntriesRequest {
                Entries = new[] {
                    @event.EncodedEntry
                }
            };

            foreach (var peerNode in _peers)
            {
                peerNode.Channel.AppendEntries(request);
            }
        }

    }
}
