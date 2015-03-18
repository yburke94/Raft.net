using System.ServiceModel;
using ProtoBuf;

namespace Raft.Service.Contracts
{
    [ServiceContract, ProtoContract]
    public interface IRaftService
    {
        [OperationContract]
        RequestVoteResponse RequestVote(RequestVoteRequest voteRequest);

        [OperationContract]
        [FaultContract(typeof(MultipleLeadersForTermFault))]
        AppendEntriesResponse AppendEntries(AppendEntriesRequest entriesRequest);
    }
}
