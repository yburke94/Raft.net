using System.Collections.Generic;
using Raft.Core.Cluster;
using Raft.Server.BufferEvents;
using Raft.Service.Contracts;
using Raft.Service.Contracts.Messages.AppendEntries;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 3 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator*
    ///     CommandFinalizer
    /// </summary>
    internal class LogReplicator : LeaderEventHandler
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
