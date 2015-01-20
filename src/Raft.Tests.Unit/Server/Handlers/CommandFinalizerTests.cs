using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class CommandFinalizerTests
    {
        [Test]
        public void CommandFinalizerDoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();
            var handler = new CommandFinalizer(raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            @event.TaskCompletionSource.Task.IsCompleted
                .Should().BeTrue();
        }

        [Test]
        public void CommandFinalizerDoesSetTaskResultWithSuccessfulLogResult()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();
            var handler = new CommandFinalizer(raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            @event.TaskCompletionSource.Task.Result
                .Should().NotBeNull();

            @event.TaskCompletionSource.Task.Result.Successful
                .Should().BeTrue();
        }

        [Test]
        public void CommandFinalizerDoesCallEntryLoggedOnRaftNode()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();

            var handler = new CommandFinalizer(raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            raftNode.Received().AddLogEntry();
        }
    }
}
