using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Raft.Core.Cluster;
using Raft.Infrastructure;
using Raft.Server.BufferEvents;

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
    internal class LogReplicator : LeaderEventHandler , IHandle<PeerJoinedCluster>, IDisposable
    {
        private volatile bool _disposing;

        private readonly BroadcastBlock<byte[]> _entryBroadcastBlock;
        private readonly ConcurrentDictionary<Guid, PeerActor> _peerNodeActors;

        public LogReplicator()
        {
            _entryBroadcastBlock = new BroadcastBlock<byte[]>(x => (byte[])x.Clone());
            _peerNodeActors = new ConcurrentDictionary<Guid, PeerActor>();
        }

        public override void Handle(CommandScheduled @event)
        {
            _entryBroadcastBlock.Post(@event.EncodedEntry);

            // TODO: Wait for (clusterSize/2) results to return successful.
        }

        public void Handle(PeerJoinedCluster @event)
        {
            if (_peerNodeActors.ContainsKey(@event.PeerInfo.NodeId) || _disposing)
                return;

            var actor = new PeerActor(@event.PeerInfo);
            actor.AddSourceLink(_entryBroadcastBlock);

            _peerNodeActors.AddOrUpdate(@event.PeerInfo.NodeId, actor, (k,v) => actor);
        }

        public void Dispose()
        {
            if (_disposing)
                throw new ObjectDisposedException(GetType().Name);

            _disposing = true;

            _peerNodeActors.Values.ToList()
                .ForEach(actor => actor.Dispose());

            _peerNodeActors.Clear();
        }
    }
}
