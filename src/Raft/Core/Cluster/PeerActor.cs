using System;
using Raft.Infrastructure;

namespace Raft.Core.Cluster
{
    internal class PeerActor : Actor<ReplicateRequest>, IDisposable
    {
        private readonly PeerInfo _peerInfo;

        public PeerActor(PeerInfo peerInfo)
        {
            _peerInfo = peerInfo;
        }

        public override void Handle(ReplicateRequest message)
        {
            // TODO: Replication and Log Matching.
        }

        public void Dispose()
        {
            CompleteActor();
        }
    }
}