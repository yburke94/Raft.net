using Raft.Core.Cluster;

namespace Raft.Core.Events
{
    internal class PeerJoinedCluster
    {
        internal PeerInfo PeerInfo { get; set; }
    }
}