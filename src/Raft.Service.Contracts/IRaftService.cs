using System.ServiceModel;
using ProtoBuf;
using Raft.Service.Contracts.Messages.AppendEntries;
using Raft.Service.Contracts.Messages.RequestVote;

namespace Raft.Service.Contracts
{
    [ServiceContract, ProtoContract]
    public interface IRaftService
    {
        [OperationContract]
        RequestVoteResponse RequestVote(RequestVoteRequest voteRequest);

        [OperationContract]
        AppendEntriesResponse AppendEntries(AppendEntriesRequest entriesRequest);
    }
}
