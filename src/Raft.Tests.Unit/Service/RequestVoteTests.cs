using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Data;
using Raft.Core.Timer;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Service;
using Raft.Service.Contracts;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Service
{
    [TestFixture]
    public class RequestVoteTests
    {
        [Test]
        public void RequestVoteAmendsTermOnRaftNodeWhenTermIsGreaterThanCurrentTerm()
        {
            // Arrange
            var message = new RequestVoteRequest
            {
                Term = 1
            };

            var raftNode = Substitute.For<INode>();
            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

            raftNode.Properties.Returns(new NodeProperties());

            // Act
            service.RequestVote(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(1);
            nodePublisher.Events[0].Command.Should().BeOfType<SetNewTerm>();
        }
    }
}
