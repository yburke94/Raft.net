using Disruptor;
using Raft.Core;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Messages.AppendEntries;
using Raft.Server.Messages.RequestVote;

namespace Raft.Server.Services
{
    internal class RaftService : IRaftService
    {
        private readonly IEventPublisher<CommitRequestedEvent> _commitPublisher;
        private readonly IEventPublisher<ApplyRequestedEvent> _applyPublisher;
        private readonly INodeTimer _timer;
        private readonly IRaftNode _raftNode;

        public RaftService(IEventPublisher<CommitRequestedEvent> commitPublisher,
            IEventPublisher<ApplyRequestedEvent> applyPublisher,
            INodeTimer timer, IRaftNode raftNode)
        {
            _commitPublisher = commitPublisher;
            _applyPublisher = applyPublisher;

            _timer = timer;
            _raftNode = raftNode;
        }

        public RequestVoteResponse RequestVote(RequestVoteRequest voteRequest)
        {
            if (_raftNode.CurrentTerm < voteRequest.Term)
                _raftNode.SetHigherTerm(voteRequest.Term);

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
                _raftNode.SetHigherTerm(entriesRequest.Term);

            return new AppendEntriesResponse
            {
                Term = _raftNode.CurrentTerm
            };
        }
    }

    internal class ApplyRequestedEvent
    {
        public long LogIdx { get; set; }

        public ApplyRequestedEvent ResetEvent(long logIdx)
        {
            // Finish impl... Refactor these out of events!!!
            return null;
        }
    }

    internal class CommitRequestedEvent
    {
        public long Term { get; set; }

        public long LogIdx { get; set; }

        public byte[] Entry { get; set; }
    }
}
