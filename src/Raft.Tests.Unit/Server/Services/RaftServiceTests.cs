using Disruptor;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.Events;
using Raft.Server.Messages.AppendEntries;
using Raft.Server.Messages.RequestVote;
using Raft.Server.Services;

namespace Raft.Tests.Unit.Server.Services
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
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new RaftLog());

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
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new RaftLog());
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
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.Log.Returns(new RaftLog());
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
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            var raftLog = new RaftLog();
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
            var raftLog = new RaftLog();
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftLog.SetLogEntry(1, 0);
            raftNode.Log.Returns(raftLog);
            raftNode.CurrentTerm.Returns(0);

            // Act
            service.AppendEntries(message);

            // Assert
            raftNode.Received(1).SetHigherTerm(message.Term);
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
            var commitPublisher = Substitute.For<IEventPublisher<CommitCommandRequested>>();
            var applyPublisher = Substitute.For<IEventPublisher<ApplyCommandRequested>>();

            var service = new RaftService(commitPublisher, applyPublisher, timer, raftNode);

            raftNode.CurrentTerm.Returns(0);

            // Act
            service.RequestVote(message);

            // Assert
            raftNode.Received(1).SetHigherTerm(message.Term);
        }
    }
}
