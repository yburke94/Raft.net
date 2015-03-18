using System.ServiceModel;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Enums;
using Raft.Core.Timer;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Service.Contracts;

namespace Raft.Service
{
    internal class RaftService : IRaftService
    {
        private readonly IPublishToBuffer<CommitCommandRequested> _commitPublisher;
        private readonly IPublishToBuffer<ApplyCommandRequested> _applyPublisher;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;
        private readonly INodeTimer _timer;
        private readonly INode _node;

        public RaftService(
            IPublishToBuffer<CommitCommandRequested> commitPublisher,
            IPublishToBuffer<ApplyCommandRequested> applyPublisher,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher,
            INodeTimer timer, INode node)
        {
            _commitPublisher = commitPublisher;
            _applyPublisher = applyPublisher;
            _nodePublisher = nodePublisher;

            _timer = timer;
            _node = node;
        }

        public RequestVoteResponse RequestVote(RequestVoteRequest voteRequest)
        {
            if (voteRequest.Term <= _node.Data.CurrentTerm)
            {
                return new RequestVoteResponse
                {
                    Term = _node.Data.CurrentTerm,
                    VoteGranted = false
                };
            }

            _nodePublisher.PublishEvent(new NodeCommandScheduled
            {
                Command = new SetNewTerm
                {
                    Term = voteRequest.Term
                }
            }).Wait();

            return null;
        }

        public AppendEntriesResponse AppendEntries(AppendEntriesRequest entriesRequest)
        {
            _timer.ResetTimer();

            if (_node.Data.CurrentTerm > entriesRequest.Term ||
                _node.Data.Log[entriesRequest.PreviousLogIndex] != entriesRequest.PreviousLogTerm)
            {
                return new AppendEntriesResponse
                {
                    Term = _node.Data.CurrentTerm,
                    Success = false
                };
            }

            if (_node.Data.CurrentTerm < entriesRequest.Term)
            {
                _nodePublisher.PublishEvent(new NodeCommandScheduled
                {
                    Command = new SetNewTerm
                    {
                        Term = entriesRequest.Term
                    }
                }).Wait();
            }

            if (_node.CurrentState == NodeState.Candidate)
            {
                _nodePublisher.PublishEvent(new NodeCommandScheduled
                {
                    Command = new CancelElection()
                }).Wait();
            }

            if (_node.CurrentState != NodeState.Follower)
                throw new FaultException<MultipleLeadersForTermFault>(new MultipleLeadersForTermFault {
                    Id = _node.Data.NodeId
                });

            _nodePublisher.PublishEvent(
                new NodeCommandScheduled {
                    Command = new SetLeaderInformation
                    {
                        LeaderId = entriesRequest.LeaderId
                    }
                });

            // TODO: Log Truncating
            // TODO: Log Writing
            // TODO: Log Applying

            return new AppendEntriesResponse
            {
                Term = _node.Data.CurrentTerm
            };
        }
    }
}
