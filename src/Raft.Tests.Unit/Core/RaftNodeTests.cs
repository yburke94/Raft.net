using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Core.Enums;
using Raft.Core.Events;
using Raft.Infrastructure;

namespace Raft.Tests.Unit.Core
{
    [TestFixture]
    public class RaftNodeTests
    {
        [Test]
        public void CanTransitionToLeaderWhenCreateClusterIsCalled()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);

            // Act
            raftNode.CreateCluster();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldReEnterLeaderStateWhenScheduleCommandExecutionIsCalled()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            raftNode.CreateCluster();

            // Act
            raftNode.ScheduleCommandExecution();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldIncreaseCommitIndexWhenCommitLogEntryIsCalledWithALogIndexGreaterThanTheCurrentCommitIndex()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            var logIdx = raftNode.CommitIndex + 1;

            TransitionNodeToState(raftNode, NodeState.Leader);

            // Act
            raftNode.CommitLogEntry(logIdx, 0L);

            // Assert
            raftNode.CommitIndex.ShouldBeEquivalentTo(logIdx);
        }

        [Test]
        public void ShouldNotIncreaseCommitIndexWhenCommitLogEntryIsCalledWithALogIndexLessThanTheCurrentCommitIndex()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            var logIdx = raftNode.CommitIndex + 1;
            var commitIdx = raftNode.CommitIndex + 2;

            TransitionNodeToState(raftNode, NodeState.Leader);
            raftNode.CommitLogEntry(commitIdx, 0L);

            raftNode.CommitIndex.Should().Be(commitIdx);

            // Act
            raftNode.CommitLogEntry(logIdx, 0L);

            // Assert
            raftNode.CommitIndex.Should().Be(commitIdx);
        }

        [Test]
        public void SpecifiedTermIsAddedToLogAtCommitIndexWhenCommitLogEntryIsCalled()
        {
            // Arrange
            const long logIdx = 1L;
            const long term = 3L;

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Follower);
            raftNode.SetTermFromRpc(term);

            // Act
            raftNode.CommitLogEntry(logIdx, term -1);

            // Assert
            raftNode.Log[raftNode.CommitIndex]
                .ShouldBeEquivalentTo(term -1);
        }

        [Test]
        public void ThrowsIfCommittingEntryAgainstATermGreaterThanTheCurrentTerm()
        {
            // Arrange
            const long term = 3L;
            const long logIdx = 1L;

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Leader);

            raftNode.CurrentTerm.Should().Be(default(long));

            // Act, Assert
            new Action(() => raftNode.CommitLogEntry(logIdx, term))
                .ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void CallingApplyCommandIncreasesLastAppliedWhenLogIdxIsGreaterThanLastAppliedIdx()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            var logIdx = raftNode.LastApplied + 1;
            TransitionNodeToState(raftNode, NodeState.Leader);

            // Act
            raftNode.ApplyCommand(logIdx);

            // Assert
            raftNode.LastApplied.Should().Be(logIdx);
        }

        [Test]
        public void CallingApplyCommandShouldNotIncreaseLastAppliedWhenLogIdxIsLessThanLastAppliedIdx()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            var lastApplied = raftNode.LastApplied + 2;
            var logIdx = raftNode.LastApplied + 1;

            TransitionNodeToState(raftNode, NodeState.Leader);
            raftNode.ApplyCommand(lastApplied);

            raftNode.LastApplied.Should().Be(lastApplied);

            // Act
            raftNode.ApplyCommand(logIdx);

            // Assert
            raftNode.LastApplied.Should().Be(lastApplied);
        }

        [Test]
        public void ShouldTransitionToFollowerStateIfCandidateAndTermSetFromRpcIsCalled()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Candidate);

            // Act
            raftNode.SetTermFromRpc(2);

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void ShouldChangeCurrentTermToNewTermWhenTermSetFromRpcIsCalled()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Follower);
            raftNode.CurrentTerm.Should().Be(0);

            // Act
            raftNode.SetTermFromRpc(2);

            // Assert
            raftNode.CurrentTerm.Should().Be(2);
        }

        [Test]
        public void ShouldThrowWhenTermSetFromRpcIsCalledAndCurrentTermIsGreaterThanSuppliedTerm()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Follower);
            raftNode.SetTermFromRpc(2);

            // Act, Assert
            new Action(() => raftNode.SetTermFromRpc(1))
                .ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ShouldPublishTermChangedEventWhenTermIsIncreasedViaRpc()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Follower);

            // Act
            raftNode.SetTermFromRpc(1);

            // Assert;
            eventDispatcher.Received().Publish(Arg.Any<TermChanged>());
        }

        [Test]
        public void JoiningClusterShouldSetStateToFollower()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            raftNode.CurrentState.Should().Be(NodeState.Initial);

            // Act
            raftNode.JoinCluster();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldTransitionNodeToCandidate()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);

            TransitionNodeToState(raftNode, NodeState.Follower);

            // Act
            raftNode.TimeoutLeaderHeartbeat();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Candidate);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldIncrementCurrentTerm()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);

            TransitionNodeToState(raftNode, NodeState.Follower);
            raftNode.CurrentTerm.Should().Be(0);

            // Act
            raftNode.TimeoutLeaderHeartbeat();

            // Assert
            raftNode.CurrentTerm.Should().Be(1);
        }

        [Test]
        public void TimingOutLeaderHeartbeatShouldIncrementCurrentTermAndPublishTermChangedEvent()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);

            TransitionNodeToState(raftNode, NodeState.Follower);
            raftNode.CurrentTerm.Should().Be(0);

            // Act
            raftNode.TimeoutLeaderHeartbeat();

            // Assert
            eventDispatcher.Received().Publish(Arg.Any<TermChanged>());
        }

        [Test]
        public void WinningCandidateElectionShouldTransitionNodeToLeader()
        {
            // Arrange
            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, NodeState.Candidate);

            // Act
            raftNode.WinCandidateElection();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [TestCase("Leader")]
        [TestCase("Follower")]
        [TestCase("Candidate", ExpectedException = typeof(AssertionException))]
        public void EnsureOnlyValidStatesCanCommitEntries(string stateString)
        {
            // Arrange
            var state = (NodeState)Enum.Parse(typeof(NodeState), stateString);

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, state);

            // Act, Assert
            new Action(() => raftNode.CommitLogEntry(1L, 0L))
                .ShouldNotThrow();
        }

        [TestCase("Leader")]
        [TestCase("Follower")]
        [TestCase("Candidate", ExpectedException = typeof(AssertionException))]
        public void EnsureOnlyValidStatesCanApplyCommands(string stateString)
        {
            // Arrange
            var state = (NodeState)Enum.Parse(typeof(NodeState), stateString);

            var eventDispatcher = Substitute.For<IEventDispatcher>();
            var raftNode = new RaftNode(eventDispatcher);
            TransitionNodeToState(raftNode, state);

            // Act, Assert
            new Action(() => raftNode.ApplyCommand(1L))
                .ShouldNotThrow();
        }

        private static void TransitionNodeToState(RaftNode node, NodeState state)
        {
            if (node.CurrentState != NodeState.Initial)
                throw new InvalidOperationException("Node must be in initial state.");

            switch (state)
            {
                case NodeState.Leader:
                    node.CreateCluster();
                    node.CurrentState.Should().Be(NodeState.Leader);
                    break;

                case NodeState.Candidate:
                    node.JoinCluster();
                    node.TimeoutLeaderHeartbeat();
                    node.CurrentState.Should().Be(NodeState.Candidate);
                    break;
                case NodeState.Follower:
                    node.JoinCluster();
                    node.CurrentState.Should().Be(NodeState.Follower);
                    break;
            }
        }
    }
}
