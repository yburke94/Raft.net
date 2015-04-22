using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Raft.Core.Cluster
{
    internal class PeerInfo
    {
        public Guid NodeId { get; set; }
        public string Address { get; set; }
    }

    internal class PeerActor : IDisposable
    {
        private readonly PeerInfo _peerInfo;
        private readonly ActionBlock<byte[]> _entriesToReplicateBlock;
        private readonly List<IDisposable> _sourceLinks;

        public PeerActor(PeerInfo peerInfo)
        {
            _peerInfo = peerInfo;
            _entriesToReplicateBlock = new ActionBlock<byte[]>(new Action<byte[]>(Replicate));
            _sourceLinks = new List<IDisposable>();
        }

        public void AddSourceLink(ISourceBlock<byte[]> source)
        {
            _sourceLinks.Add(source.LinkTo(_entriesToReplicateBlock));
        }

        private void Replicate(byte[] entry)
        {
            // TODO: Replication and LogMatching logic.
        }

        public void Dispose()
        {
            _entriesToReplicateBlock.Complete();
            _entriesToReplicateBlock.Completion.Wait();

            _sourceLinks.ForEach(x => x.Dispose());
        }
    }

    internal class PeerJoinedCluster
    {
        internal PeerInfo PeerInfo { get; set; }
    }
}
