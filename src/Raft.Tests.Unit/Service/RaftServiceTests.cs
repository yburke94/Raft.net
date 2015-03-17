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
using Raft.Server.Handlers.Core;
using Raft.Service;
using Raft.Service.Contracts.Messages.AppendEntries;
using Raft.Service.Contracts.Messages.RequestVote;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Service
{
    [TestFixture]
    public class RaftServiceTests
    {
        [Test]
        public void AppendEntriesResetsTimer()
        {
            // Arrange
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(new NodeData { Log = new NodeLog() });

            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            // Act
            service.AppendEntries(message);

            // Assert
            timer.Received(1).ResetTimer();
        }

        [Test]
        public void AppendEntriesReturnsNodesCurrentTerm()
        {
            // Arrange
            const int expectedTerm = 456;
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<INode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            raftNode.Data.Returns(new NodeData
            {
                Log = new NodeLog(),
                CurrentTerm = expectedTerm
            });

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Term.ShouldBeEquivalentTo(expectedTerm);
        }

        [Test]
        public void AppendEntriesIsUnsuccessfulIfLeaderTermIsLessThanCurrentTerm()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 234
            };

            var nodeData = new NodeData
            {
                CurrentTerm = message.Term + 10,
                Log = new NodeLog()
            };

            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Success.Should().BeFalse(
                "because node cannot apply log " +
                "if the leader term is less than the nodes term");
        }

        [Test]
        public void AppendEntriesReturnsFalseIfPrevEntryTermDoesNotMatch()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                PreviousLogIndex = 0,
                PreviousLogTerm = 1
            };

            var raftNode = Substitute.For<INode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            var raftLog = new NodeLog();
            raftLog.SetLogEntry(1, 2L);

            raftNode.Data.Returns(new NodeData { Log = raftLog });

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Success.Should().BeFalse(
                "because node cannot apply log if the term " +
                "for the leaders previous log index does not match.");
        }

        [Test]
        public void AppendEntriesAmendsTermOnRaftNodeWhenTermIsGreaterThanCurrentTerm()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 1,
                PreviousLogIndex = 1,
                PreviousLogTerm = 0
            };

            var raftNode = Substitute.For<INode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            var nodeData = new NodeData
            {
                Log = new NodeLog()
            };
            
            nodeData.Log.SetLogEntry(1, 0);
            raftNode.Data.Returns(nodeData);

            // Act
            service.AppendEntries(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(1);
            nodePublisher.Events[0].Command.Should().BeOfType<SetNewTerm>();
        }

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
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(commitPublisher, applyPublisher, nodePublisher, timer, raftNode);

            raftNode.Data.Returns(new NodeData());

            // Act
            service.RequestVote(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(1);
            nodePublisher.Events[0].Command.Should().BeOfType<SetNewTerm>();
        }
    }
}
