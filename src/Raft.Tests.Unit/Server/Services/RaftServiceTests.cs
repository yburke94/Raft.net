using FluentAssertions;
using NSubstitute;
using NSubstitute.Exceptions;
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
        public void CallingAppendEntriesResetsTimer()
        {
            // Arrange
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var service = new RaftService(timer, raftNode);

            // Act
            service.AppendEntries(message);

            // Assert
            timer.Received(1).ResetTimer();
        }

        [Test]
        public void CallingAppendEntriesReturnsNodesCurrentTerm()
        {
            // Arrange
            const int expectedTerm = 456;
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<IRaftNode>();
            var timer = Substitute.For<INodeTimer>();
            var service = new RaftService(timer, raftNode);

            raftNode.CurrentLogTerm.Returns(expectedTerm);

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Term.ShouldBeEquivalentTo(expectedTerm);
        }


    }
}
