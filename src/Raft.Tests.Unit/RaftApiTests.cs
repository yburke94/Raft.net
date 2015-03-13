using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Enums;
using Raft.Exceptions;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit
{
    [TestFixture]
    public class RaftApiTests
    {
        [TestCase(NodeState.Candidate)]
        [TestCase(NodeState.Follower)]
        [TestCase(NodeState.Leader, ExpectedException = typeof(AssertionException))]
        public void ThrowsWhenExecutingCommandAndNotALeader(NodeState state)
        {
            // Arrange
            var publishToBuffer = Substitute.For<IPublishToBuffer<CommandScheduled>>();
            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentState.Returns(state);
            var raftApi = new RaftApi(publishToBuffer, raftNode);

            // Act
            Action actAction = () => raftApi.ExecuteCommand(new TestCommand());

            // Assert
            actAction.ShouldThrow<NotClusterLeaderException>();
        }
    }
}
