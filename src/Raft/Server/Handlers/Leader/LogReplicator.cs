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

        private readonly IPeerActorFactory _peerActorFactory;

        private readonly BroadcastBlock<ReplicateRequest> _entryBroadcastBlock;
        private readonly ConcurrentDictionary<Guid, Actor<ReplicateRequest>> _replicationActors;

        public LogReplicator(IPeerActorFactory peerActorFactory)
        {
            _peerActorFactory = peerActorFactory;
            _entryBroadcastBlock = new BroadcastBlock<ReplicateRequest>(x => x.Clone());
            _replicationActors = new ConcurrentDictionary<Guid, Actor<ReplicateRequest>>();
        }

        public override void Handle(CommandScheduled @event)
        {
            var peerCount = _replicationActors.Count;
            if (peerCount < 1)
                throw new InvalidOperationException(
                    "Attempted to replicate message with no peers in the cluster.");

            var replicatedCounter = new WaitableCounter((int)Math.Ceiling((decimal)peerCount/2));

            var replicationRequest = new ReplicateRequest(
                @event.EncodedEntry, () => replicatedCounter.Increment());

            _entryBroadcastBlock.Post(replicationRequest);

            replicatedCounter.Wait();
        }

        public void Handle(PeerJoinedCluster @event)
        {
            if (_replicationActors.ContainsKey(@event.PeerInfo.NodeId) || _disposing)
                return;

            var actor = _peerActorFactory.Create(@event.PeerInfo);
            actor.AddSourceLink(_entryBroadcastBlock);

            _replicationActors.AddOrUpdate(@event.PeerInfo.NodeId, actor, (k,v) => actor);
        }

        public void Dispose()
        {
            if (_disposing) return;

            _disposing = true;

            _entryBroadcastBlock.Complete();
            _entryBroadcastBlock.Completion.Wait();

            _replicationActors.Values.ToList()
                .ForEach(actor => {
                    var disposable = actor as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                });

            _replicationActors.Clear();
        }
    }
}
