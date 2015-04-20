using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raft.Server.BufferEvents;
using Raft.Service.Contracts;

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
        private readonly IInternalPeerService _peerService;

        public LogReplicator(IInternalPeerService peerService)
        {
            _peerService = peerService;
        }

        public override void Handle(CommandScheduled @event)
        {
            var peers = _peerService.GetPeersInCluster();
            foreach (var peer in peers)
            {
                var replicationRequest = new ReplicationRequest
                {
                    NodeId = peer.NodeId,
                    EndpointAddress = peer.Address,
                    Entry = @event.EncodedEntry
                };

                Task.Factory.StartNew(() => ReplicateToNode(replicationRequest),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default);
            }
        }

        public ReplicationResult ReplicateToNode(ReplicationRequest request)
        {
            return new ReplicationResult
            {
                NodeId = request.NodeId,
                Success = true
            };
        }

        public class ReplicationRequest
        {
            public Guid NodeId { get; set; }

            public string EndpointAddress { get; set; }

            public byte[] Entry { get; set; }
        }

        public class ReplicationResult
        {
            public Guid NodeId { get; set; }
            public bool Success { get; set; }
        }
    }
}
