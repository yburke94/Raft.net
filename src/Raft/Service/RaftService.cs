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
        
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;
        private readonly INodeTimer _timer;
        private readonly INode _node;

        public RaftService(
            IPublishToBuffer<CommitCommandRequested> commitPublisher,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher,
            INodeTimer timer, INode node)
        {
            _commitPublisher = commitPublisher;
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
            // If the node term is greater, return before updating timer. Eventually an election will trigger.
            if (_node.Data.CurrentTerm > entriesRequest.Term)
                return new AppendEntriesResponse
                {
                    Term = _node.Data.CurrentTerm,
                    Success = false
                };

            _timer.ResetTimer();

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
                throw new FaultException<MultipleLeadersForTermFault>(new MultipleLeadersForTermFault
                {
                    Id = _node.Data.NodeId
                });

            if (_node.Data.Log[entriesRequest.PreviousLogIndex] != entriesRequest.PreviousLogTerm)
            {
                return new AppendEntriesResponse
                {
                    Term = _node.Data.CurrentTerm,
                    Success = false
                };
            }

            _nodePublisher.PublishEvent(
                new NodeCommandScheduled {
                    Command = new SetLeaderInformation
                    {
                        LeaderId = entriesRequest.LeaderId
                    }
                });

            // TODO: Buffer will be responsible for Log Truncating, Log Writing, Log Applying
            _commitPublisher.PublishEvent(new CommitCommandRequested
            {
                PreviousLogIndex = entriesRequest.PreviousLogIndex,
                PreviousLogTerm = entriesRequest.PreviousLogTerm,
                LeaderCommit = entriesRequest.LeaderCommit,
                Entries = entriesRequest.Entries
            });

            return new AppendEntriesResponse
            {
                Term = _node.Data.CurrentTerm
            };
        }
    }
}
