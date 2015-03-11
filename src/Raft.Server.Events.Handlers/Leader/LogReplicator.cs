using System.Collections.Generic;
using Raft.Core.Cluster;
using Raft.Service.Contracts;
using Raft.Service.Contracts.Messages.AppendEntries;

namespace Raft.Server.Events.Handlers.Leader
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
    public class LogReplicator : LeaderEventHandler, ISkipInternalCommands
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
                GetChannel(peerNode).AppendEntries(request);
            }
        }

        // TODO: Get service channel from peer node.
        private IRaftService GetChannel(PeerNode node)
        {
            return default(IRaftService);
        }

    }
}
