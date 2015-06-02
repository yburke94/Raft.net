namespace Raft.Core.Commands
{
    internal class JoinCluster : INodeCommand {
        public long ClusterTerm { get; set; }
    }
}