using Raft.Core.StateMachine;
using Raft.Core.Timer;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Events;
using Raft.Service.Contracts;
using Raft.Service.Contracts.Messages.AppendEntries;
using Raft.Service.Contracts.Messages.RequestVote;

namespace Raft.Service
{
    public class RaftService : IRaftService
    {
        private readonly IPublishToBuffer<CommitCommandRequested> _commitPublisher;
        private readonly IPublishToBuffer<ApplyCommandRequested> _applyPublisher;
        private readonly INodeTimer _timer;
        private readonly IRaftNode _raftNode;

        public RaftService(IPublishToBuffer<CommitCommandRequested> commitPublisher,
            IPublishToBuffer<ApplyCommandRequested> applyPublisher,
            INodeTimer timer, IRaftNode raftNode)
        {
            _commitPublisher = commitPublisher;
            _applyPublisher = applyPublisher;

            _timer = timer;
            _raftNode = raftNode;
        }

        public RequestVoteResponse RequestVote(RequestVoteRequest voteRequest)
        {
            if (voteRequest.Term <= _raftNode.CurrentTerm)
            {
                return new RequestVoteResponse
                {
                    Term = _raftNode.CurrentTerm,
                    VoteGranted = false
                };
            }
            else
            {
                _raftNode.SetTermFromRpc(voteRequest.Term);
            }

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

            if (_raftNode.CurrentTerm < entriesRequest.Term)
                _raftNode.SetTermFromRpc(entriesRequest.Term);

            return new AppendEntriesResponse
            {
                Term = _raftNode.CurrentTerm
            };
        }
    }
}
