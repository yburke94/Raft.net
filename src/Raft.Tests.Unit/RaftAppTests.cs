using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Contracts;
using Raft.Core.StateMachine;
using Raft.Exceptions;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit
{
    [TestFixture]
    public class RaftAppTests
    {
        [TestCase(NodeState.Candidate)]
        [TestCase(NodeState.Follower)]
        [TestCase(NodeState.Leader, ExpectedException = typeof(AssertionException))]
        public void ThrowsWhenExecutingCommandAndNotALeader(NodeState state)
        {
            // Arrange
            var publishToBuffer = Substitute.For<IPublishToBuffer<CommandScheduled>>();
            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(state);
            var raftApi = new RaftApp(publishToBuffer, raftNode);

            // Act
            Action actAction = () => raftApi.ExecuteCommand(new TestCommand());

            // Assert
            actAction.ShouldThrow<NotClusterLeaderException>();
        }
    }
}
