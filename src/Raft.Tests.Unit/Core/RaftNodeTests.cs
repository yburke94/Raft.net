using System;
using FluentAssertions;
using NUnit.Framework;
using Raft.Core;

namespace Raft.Tests.Unit.Core
{
    [TestFixture]
    public class RaftNodeTests
    {
        [Test]
        public void CanTransitionToLeaderWhenCreateClusterIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();

            // Act
            raftNode.CreateCluster();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldReEnterLeaderStateWhenScheduleCommandExecutionIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
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
            var raftNode = new RaftNode();
            var logIdx = raftNode.CommitIndex + 1;

            raftNode.CreateCluster();

            // Act
            raftNode.CommitLogEntry(logIdx);

            // Assert
            raftNode.CommitIndex.ShouldBeEquivalentTo(logIdx);
        }

        [Test]
        public void ShouldNotIncreaseCommitIndexWhenCommitLogEntryIsCalledWithALogIndexLessThanTheCurrentCommitIndex()
        {
            // Arrange
            var raftNode = new RaftNode();
            var logIdx = raftNode.CommitIndex + 1;
            var commitIdx = raftNode.CommitIndex + 2;

            raftNode.CreateCluster();
            raftNode.CommitLogEntry(commitIdx);

            raftNode.CommitIndex.Should().Be(commitIdx);

            // Act
            raftNode.CommitLogEntry(logIdx);

            // Assert
            raftNode.CommitIndex.Should().Be(commitIdx);
        }

        [Test]
        public void CurrentTermIsAddedToLogAtCommitIndexWhenCommitLogEntryIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            var logIdx = raftNode.CommitIndex + 1;
            raftNode.CreateCluster();

            // Act
            raftNode.CommitLogEntry(logIdx);

            // Assert
            raftNode.Log[raftNode.CommitIndex]
                .ShouldBeEquivalentTo(raftNode.CurrentTerm);
        }

        [Test]
        public void CallingApplyCommandIncreasesLastAppliedWhenLogIdxIsGreaterThanLastAppliedIdx()
        {
            // Arrange
            var raftNode = new RaftNode();
            var logIdx = raftNode.LastApplied + 1;
            raftNode.CreateCluster();

            // Act
            raftNode.ApplyCommand(logIdx);

            // Assert
            raftNode.LastApplied.Should().Be(logIdx);
        }

        [Test]
        public void CallingApplyCommandShouldNotIncreaseLastAppliedWhenLogIdxIsLessThanLastAppliedIdx()
        {
            // Arrange
            var raftNode = new RaftNode();
            var lastApplied = raftNode.LastApplied + 2;
            var logIdx = raftNode.LastApplied + 1;

            raftNode.CreateCluster();
            raftNode.ApplyCommand(lastApplied);

            raftNode.LastApplied.Should().Be(lastApplied);

            // Act
            raftNode.ApplyCommand(logIdx);

            // Assert
            raftNode.LastApplied.Should().Be(lastApplied);
        }

        [Test]
        public void ShouldTransitionToFollowerStateIfNotAlreadyAndSetHigherTermIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();
            raftNode.CurrentState.Should().Be(NodeState.Leader);

            // Act
            raftNode.SetHigherTerm(2);

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void ShouldChangeCurrentTermToNewTermWhenSetHigherTermIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();
            raftNode.CurrentTerm.Should().Be(0);

            // Act
            raftNode.SetHigherTerm(2);

            // Assert
            raftNode.CurrentTerm.Should().Be(2);
        }

        [Test]
        public void ShouldThrowWhenSetHigherTermIsCalledAndCurrentTermIsGreaterThanSuppliedTerm()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();
            raftNode.SetHigherTerm(2);

            // Act, Assert
            new Action(() => raftNode.SetHigherTerm(1))
                .ShouldThrow<InvalidOperationException>();
        }
    }
}
