using Raft.Core;
using Raft.Server.Messages.AppendEntries;
using Raft.Server.Messages.RequestVote;

namespace Raft.Server.Services
{
    internal class RaftService : IRaftService
    {
        private readonly INodeTimer _timer;
        private readonly IRaftNode _raftNode;

        public RaftService(INodeTimer timer, IRaftNode raftNode)
        {
            _timer = timer;
            _raftNode = raftNode;
        }

        public RequestVoteResponse RequestVote(RequestVoteRequest voteRequest)
        {
            return null;
        }

        public AppendEntriesResponse AppendEntries(AppendEntriesRequest entriesRequest)
        {
            _timer.ResetTimer();

            if (_raftNode.CurrentTerm > entriesRequest.Term ||
                _raftNode.Log[entriesRequest.PreviousLogIndex] != entriesRequest.PreviousLogTerm)
            {
                return new AppendEntriesResponse
                {
                    Term = _raftNode.CurrentTerm,
                    Success = false
                };
            }

            return new AppendEntriesResponse
            {
                Term = _raftNode.CurrentTerm
            };
        }
    }
}
