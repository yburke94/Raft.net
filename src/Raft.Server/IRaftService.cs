using System.ServiceModel;
using ProtoBuf;
using Raft.Server.Messages.AppendEntries;
using Raft.Server.Messages.RequestVote;

namespace Raft.Server
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
