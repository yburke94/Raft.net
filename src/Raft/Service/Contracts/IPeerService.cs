using System.ServiceModel;
using ProtoBuf;

namespace Raft.Service.Contracts
{
    // Public WCF Contract
    [ServiceContract, ProtoContract]
    public interface IPeerService
    {
    }
}
