using System;

namespace Raft.Core.Cluster
{
    internal class Peer
    {
        public Guid NodeId { get; set; }

        public string Address { get; set; }

    }
}
