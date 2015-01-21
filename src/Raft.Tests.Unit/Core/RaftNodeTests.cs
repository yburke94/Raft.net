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
        public void ShouldReEnterLeaderStateWhenExecuteCommandIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.ExecuteCommand();

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
    }
}
