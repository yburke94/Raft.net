using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Core.Log;
using Raft.Core.StateMachine;
using Raft.Core.Timer;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.Events;
using Raft.Service;
using Raft.Service.Contracts.Messages.AppendEntries;
using Raft.Service.Contracts.Messages.RequestVote;

namespace Raft.Tests.Unit.Server.Service
{
    [TestFixture]
    public class RaftServiceTests
    {
        [Test]
        public void AppendEntriesResetsTimer()
        {
            // Arrange
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new NodeLog());

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

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new NodeLog());
            raftNode.CurrentTerm.Returns(expectedTerm);

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

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new NodeLog());
            raftNode.CurrentTerm.Returns(message.Term + 10);

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

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            var raftLog = new NodeLog();
            raftLog.SetLogEntry(1, 2L);

            raftNode.Log.Returns(raftLog);

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

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var raftLog = new NodeLog();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftLog.SetLogEntry(1, 0);
            raftNode.Log.Returns(raftLog);
            raftNode.CurrentTerm.Returns(0);

            // Act
            service.AppendEntries(message);

            // Assert
            raftNode.Received(1).SetTermFromRpc(message.Term);
        }

        [Test]
        public void RequestVoteAmendsTermOnRaftNodeWhenTermIsGreaterThanCurrentTerm()
        {
            // Arrange
            var message = new RequestVoteRequest
            {
                Term = 1
            };

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var commitPublisher = Substitute.For<IPublishToBuffer<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IPublishToBuffer<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.CurrentTerm.Returns(0);

            // Act
            service.RequestVote(message);

            // Assert
            raftNode.Received(1).SetTermFromRpc(message.Term);
        }
    }
}
