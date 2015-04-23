using System;

namespace Raft.Core.Cluster
{
    internal class PeerInfo
    {
        public Guid NodeId { get; set; }
        public string Address { get; set; }
    }
}
