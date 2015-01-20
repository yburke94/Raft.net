using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server;
using Raft.Server.Messages.AppendEntries;
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
            var service = new RaftService(timer, raftNode);

            raftNode.Log.Returns(new long?[64]);

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
            var service = new RaftService(timer, raftNode);

            raftNode.Log.Returns(new long?[64]);
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
            var service = new RaftService(timer, raftNode);

            raftNode.Log.Returns(new long?[64]);
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
            var service = new RaftService(timer, raftNode);

            raftNode.Log.Returns(new long?[]{2L});

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Success.Should().BeFalse(
                "because node cannot apply log if the term " +
                "for the leaders previous log index does not match.");
        }



    }
}
