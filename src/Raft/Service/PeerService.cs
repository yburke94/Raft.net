using System.Collections.Generic;
using Raft.Core.Cluster;
using Raft.Service.Contracts;

namespace Raft.Service
{
    internal class PeerService : IPeerService, IInternalPeerService
    {
        // TODO: Impl
        public IList<Peer> GetPeersInCluster()
        {
            return new List<Peer>
            {
                new Peer(),
                new Peer(),
                new Peer()
            };
        }
    }
}
