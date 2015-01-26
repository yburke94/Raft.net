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
        public void ShouldIncrementCommitIndexWhenAddLogEntryIsCalledEverytime()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.AddLogEntry();

            // Assert
            raftNode.CommitIndex.ShouldBeEquivalentTo(1);
        }

        [Test]
        public void CurrentTermIsAddedToLogAtCommitIndexWhenAddLogEntryIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.AddLogEntry();

            // Assert
            raftNode.Log[raftNode.CommitIndex]
                .ShouldBeEquivalentTo(raftNode.CurrentTerm);
        }

        [Test]
        public void CallingApplyCommandIncrementsLastApplied()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.ApplyCommand();

            // Assert
            raftNode.LastApplied.Should().Be(1);
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
