using FluentAssertions;
using NUnit.Framework;
using Raft.Core;

namespace Raft.Tests.Unit.Core
{
    [TestFixture]
    public class RaftNodeTests
    {
        [Test]
        public void CanTransitionToLeaderWhenJoinClusterIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();

            // Act
            raftNode.JoinCluster();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldReEnterLeaderStateLogEntryIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.JoinCluster();

            // Act
            raftNode.LogEntry();

            // Assert
            raftNode.CurrentState.Should().Be(NodeState.Leader);
        }

        [Test]
        public void ShouldNotIncrementLastLogIndexWhenEntryLoggedIsCalledForTheFirstTime()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.JoinCluster();

            // Act
            raftNode.EntryLogged();

            // Assert
            raftNode.LastLogIndex.ShouldBeEquivalentTo(0);
        }

        [Test]
        public void ShouldIncrementLastLogIndexWhenEntryLoggedIsCalledEveryTimeAfterTheFirstTime()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.JoinCluster();

            // Act
            raftNode.EntryLogged();
            raftNode.EntryLogged();
            raftNode.EntryLogged();

            // Assert
            raftNode.LastLogIndex.ShouldBeEquivalentTo(2);
        }

        [Test]
        public void CurrentTermIsAddedToLogAtLastLogIndexWhenEntryLoggedIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.JoinCluster();

            // Act
            raftNode.EntryLogged();

            // Assert
            raftNode.Log[raftNode.LastLogIndex]
                .ShouldBeEquivalentTo(raftNode.CurrentTerm);
        }

        [Test]
        public void IncreaseLogLengthBy64WhenPreviousLogIndexEqualsLengthAndEntryLoggedIsCalled()
        {
            // Arrange
            var raftNode = new RaftNode();
            raftNode.JoinCluster();

            for (var i = 0; i < 64; i++)
                raftNode.EntryLogged();

            raftNode.Log.Length.ShouldBeEquivalentTo(64);

            // Act
            raftNode.EntryLogged();

            // Assert
            raftNode.Log.Length.ShouldBeEquivalentTo(128);
        }
    }
}
