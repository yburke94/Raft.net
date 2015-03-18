using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.Events;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Enums;
using Raft.Infrastructure;

namespace Raft.Tests.Unit.Core.StateMachine
{
    [TestFixture]
    public class RaftNodeTests
    {
        [Test]
        public void CanTransitionToLeaderHandlingCreateClusterCmd()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);

            // Act
            raftNode.FireAtStateMachine<CreateCluster>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldIncreaseCommitIndexWhenHandlingCommitEntryCmdWithALogIndexGreaterThanTheCurrentCommitIndex()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            var logIdx = raftNode.Data.CommitIndex + 1;

            TransitionNodeFromInitialState(raftNode, NodeState.Leader);

            // Act
            raftNode.Handle(new CommitEntry
            {
                EntryIdx = logIdx,
                EntryTerm = 0L
            });

            // Assert
            raftNode.Data.CommitIndex.ShouldBeEquivalentTo(logIdx);
        }

        [Test]
        public void ShouldNotIncreaseCommitIndexWhenHandlingCommitEntryCmdWithALogIndexLessThanTheCurrentCommitIndex()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            var logIdx = raftNode.Data.CommitIndex + 1;
            var commitIdx = raftNode.Data.CommitIndex + 2;

            TransitionNodeFromInitialState(raftNode, NodeState.Leader);
            raftNode.Handle(new CommitEntry
            {
                EntryIdx = commitIdx,
                EntryTerm = 0L
            });

            raftNode.Data.CommitIndex.Should().Be(commitIdx);

            // Act
            raftNode.Handle(new CommitEntry
            {
                EntryIdx = logIdx,
                EntryTerm = 0L
            });

            // Assert
            raftNode.Data.CommitIndex.Should().Be(commitIdx);
        }

        [Test]
        public void SpecifiedTermIsAddedToLogAtCommitIndexWhenHandlingCommitEntryCmd()
        {
            // Arrange
            const long logIdx = 1L;
            const long term = 3L;

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Handle(new SetNewTerm
            {
                Term = term
            });

            // Act
            raftNode.Handle(new CommitEntry
            {
                EntryIdx = logIdx,
                EntryTerm = term-1
            });

            // Assert
            raftNode.Data.Log[raftNode.Data.CommitIndex]
                .ShouldBeEquivalentTo(term -1);
        }

        [Test]
        public void ThrowsIfCommittingEntryAgainstATermGreaterThanTheCurrentTerm()
        {
            // Arrange
            const long term = 3L;
            const long logIdx = 1L;

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Leader);

            raftNode.Data.CurrentTerm.Should().Be(default(long));

            // Act, Assert
            new Action(() => raftNode.Handle(
                new CommitEntry
                {
                    EntryIdx = logIdx,
                    EntryTerm = term 
                })).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void HandlingApplyEntryCmdIncreasesLastAppliedWhenLogIdxIsGreaterThanLastAppliedIdx()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            var logIdx = raftNode.Data.LastApplied + 1;
            TransitionNodeFromInitialState(raftNode, NodeState.Leader);

            // Act
            raftNode.Handle(new ApplyEntry
            {
                EntryIdx = logIdx
            });

            // Assert
            raftNode.Data.LastApplied.Should().Be(logIdx);
        }

        [Test]
        public void HandlingApplyEntryCmdShouldNotIncreaseLastAppliedWhenLogIdxIsLessThanLastAppliedIdx()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            var lastApplied = raftNode.Data.LastApplied + 2;
            var logIdx = raftNode.Data.LastApplied + 1;

            TransitionNodeFromInitialState(raftNode, NodeState.Leader);
            raftNode.Handle(new ApplyEntry
            {
                EntryIdx = lastApplied
            });

            raftNode.Data.LastApplied.Should().Be(lastApplied);

            // Act
            raftNode.Handle(new ApplyEntry
            {
                EntryIdx = logIdx
            });

            // Assert
            raftNode.Data.LastApplied.Should().Be(lastApplied);
        }

        [TestCase(NodeState.Leader)]
        [TestCase(NodeState.Candidate)]
        [TestCase(NodeState.Follower)]
        public void ShouldTransitionToFollowerStateIfCandidateAndHandlingSetNewTerm(NodeState initial)
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, initial);

            // Act
            raftNode.FireAtStateMachine<SetNewTerm>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void ShouldChangeCurrentTermToNewTermWhenHandlingSetNewTerm()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Data.CurrentTerm.Should().Be(0);

            // Act
            raftNode.Handle(new SetNewTerm
            {
                Term = 2
            });

            // Assert
            raftNode.Data.CurrentTerm.Should().Be(2);
        }

        [Test]
        public void ShouldThrowWhenHandlingSetNewTermAndCurrentTermIsGreaterThanSuppliedTerm()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Handle(new SetNewTerm
            {
                Term = 2
            });

            // Act, Assert
            new Action(() => raftNode.Handle(new SetNewTerm
            {
                Term = 1
            })).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ShouldPublishTermChangedEventWhenTermIsIncreased()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Follower);

            // Act
            raftNode.Handle(new SetNewTerm
            {
                Term = 1
            });

            // Assert;
            eventDispatcher.Received().Publish(Arg.Any<TermChanged>());
        }

        [Test]
        public void JoiningClusterShouldSetStateToFollower()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            raftNode.CurrentState.Should().Be(NodeState.Initial);

            // Act
            raftNode.FireAtStateMachine<JoinCluster>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldTransitionNodeToCandidate()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);

            TransitionNodeFromInitialState(raftNode, NodeState.Follower);

            // Act
            raftNode.FireAtStateMachine<TimeoutLeaderHeartbeat>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Candidate);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldIncrementCurrentTerm()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);

            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Data.CurrentTerm.Should().Be(0);

            // Act
            raftNode.Handle(new TimeoutLeaderHeartbeat());

            // Assert
            raftNode.Data.CurrentTerm.Should().Be(1);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldIncrementCurrentTermAndPublishTermChangedEvent()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);

            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Data.CurrentTerm.Should().Be(0);

            // Act
            raftNode.Handle(new TimeoutLeaderHeartbeat());;

            // Assert
            eventDispatcher.Received().Publish(Arg.Any<TermChanged>());
        }

        [Test]
        public void WinningCandidateElectionShouldTransitionNodeToLeader()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Candidate);

            // Act
            raftNode.FireAtStateMachine<WinCandidateElection>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void CancellingElectionWhenCandidateWillTransitionNodeToFollower()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Candidate);

            // Act
            raftNode.FireAtStateMachine<CancelElection>();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [TestCase("Leader")]
        [TestCase("Follower")]
        [TestCase("Candidate", ExpectedException = typeof(AssertionException))]
        public void EnsureOnlyValidStatesCanCommitEntries(string stateString)
        {
            // Arrange
            var state = (NodeState)Enum.Parse(typeof(NodeState), stateString);

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, state);

            // Act, Assert
            new Action(() => raftNode.FireAtStateMachine<CommitEntry>()).ShouldNotThrow();
        }

        [TestCase("Leader")]
        [TestCase("Follower")]
        [TestCase("Candidate", ExpectedException = typeof(AssertionException))]
        public void EnsureOnlyValidStatesCanApplyCommands(string stateString)
        {
            // Arrange
            var state = (NodeState)Enum.Parse(typeof(NodeState), stateString);

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, state);

            // Act, Assert
            new Action(() => raftNode.FireAtStateMachine<ApplyEntry>()).ShouldNotThrow();
        }

        [Test]
        public void HandlingSetLeaderInformationSetLeaderIdInNodeData()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Candidate);
            raftNode.Data.LeaderId.Should().BeEmpty();

            var command = new SetLeaderInformation
            {
                LeaderId = Guid.NewGuid()
            };

            // Act
            raftNode.Handle(command);

            // Assert
            raftNode.Data.LeaderId.Should().Be(command.LeaderId);
        }

        [TestCase(NodeState.Leader)]
        [TestCase(NodeState.Candidate)]
        public void LeaderIdIsSetToEmptyWhenNodeTransitionsFromAFollower(NodeState newState)
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new Node(eventDispatcher);
            TransitionNodeFromInitialState(raftNode, NodeState.Follower);
            raftNode.Handle(new SetLeaderInformation {LeaderId = Guid.NewGuid()});
            raftNode.Data.LeaderId.Should().NotBeEmpty();

            // Act
            if (newState == NodeState.Leader)
            {
                raftNode.FireAtStateMachine<TimeoutLeaderHeartbeat>();
                raftNode.FireAtStateMachine<WinCandidateElection>();
            }
            else if (newState == NodeState.Candidate)
            {
                raftNode.FireAtStateMachine<TimeoutLeaderHeartbeat>();
            }

            raftNode.CurrentState.Should().Be(newState);

            // Assert
            raftNode.Data.LeaderId.Should().BeEmpty();
        }

        private static void TransitionNodeFromInitialState(Node node, NodeState state)
        {
            if (node.CurrentState != NodeState.Initial)
                throw new InvalidOperationException("Node must be in initial state.");

            switch (state)
            {
                case NodeState.Leader:
                    node.FireAtStateMachine<CreateCluster>();
                    node.CurrentState.Should().Be(NodeState.Leader);
                    break;

                case NodeState.Candidate:
                    node.FireAtStateMachine<JoinCluster>();
                    node.FireAtStateMachine<TimeoutLeaderHeartbeat>();
                    node.CurrentState.Should().Be(NodeState.Candidate);
                    break;
                case NodeState.Follower:
                    node.FireAtStateMachine<JoinCluster>();
                    node.CurrentState.Should().Be(NodeState.Follower);
                    break;
            }
        }
    }
}
