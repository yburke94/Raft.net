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
        public void ShouldNotIncrementLastLogIndexWhenAddLogEntryIsCalledForTheFirstTime()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.AddLogEntry();

            // Assert
            raftNode.LastLogIndex.ShouldBeEquivalentTo(0);
        }

        [Test]
        public void ShouldIncrementLastLogIndexWhenAddLogEntryIsCalledEveryTimeAfterTheFirstTime()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.AddLogEntry();
            raftNode.AddLogEntry();
            raftNode.AddLogEntry();

            // Assert
            raftNode.LastLogIndex.ShouldBeEquivalentTo(2);
        }

        [Test]
        public void CurrentTermIsAddedToLogAtLastLogIndexWhenAddLogEntryIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            // Act
            raftNode.AddLogEntry();

            // Assert
            raftNode.Log[raftNode.LastLogIndex]
                .ShouldBeEquivalentTo(raftNode.CurrentTerm);
        }

        [Test]
        public void IncreaseLogLengthBy64WhenPreviousLogIndexEqualsLengthAndAddLogEntryIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.CreateCluster();

            for (var i = 0; i < 64; i++)
                raftNode.AddLogEntry();

            raftNode.Log.Length.ShouldBeEquivalentTo(64);

            // Act
            raftNode.AddLogEntry();

            // Assert
            raftNode.Log.Length.ShouldBeEquivalentTo(128);
        }
    }
}
