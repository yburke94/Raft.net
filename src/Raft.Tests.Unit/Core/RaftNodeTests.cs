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
    }
}
