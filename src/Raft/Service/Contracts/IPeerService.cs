using System.Collections.Generic;
using System.ServiceModel;
using ProtoBuf;
using Raft.Core.Cluster;

namespace Raft.Service.Contracts
{
    // Public WCF Contract
    [ServiceContract, ProtoContract]
    public interface IPeerService
    {
    }

    // Internal Contract
    internal interface IInternalPeerService
    {
        IList<Peer> GetPeersInCluster();
    }
}
