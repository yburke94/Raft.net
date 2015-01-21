using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class CommandApplierTests
    {
        [Test]
        public void DoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();
            var raftNode = Substitute.For<IRaftNode>();
            var context = new RaftServerContext();

            var handler = new CommandApplier(raftNode, context);

            // Act
            handler.Handle(@event);

            // Assert
            @event.TaskCompletionSource.Task.IsCompleted
                .Should().BeTrue();
        }

        [Test]
        public void DoesSetTaskResultWithSuccessfulLogResult()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();
            var raftNode = Substitute.For<IRaftNode>();
            var context = new RaftServerContext();

            var handler = new CommandApplier(raftNode, context);

            // Act
            handler.Handle(@event);

            // Assert
            @event.TaskCompletionSource.Task.Result
                .Should().NotBeNull();

            @event.TaskCompletionSource.Task.Result.Successful
                .Should().BeTrue();
        }

        [Test]
        public void DoesExecuteCommand()
        {
            // Arrange
            var shouldEqualTrueWhenCommandExecutes = false;

            var @event = TestEventFactory.GetCommandEvent(
                () => shouldEqualTrueWhenCommandExecutes = true);

            var raftNode = Substitute.For<IRaftNode>();
            var context = new RaftServerContext();

            var handler = new CommandApplier(raftNode, context);

            // Act
            handler.Handle(@event);

            // Assert
            shouldEqualTrueWhenCommandExecutes
                .Should().BeTrue();
        }

        [Test]
        public void DoesCallCommandAppliedOnRaftNode()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var raftNode = Substitute.For<IRaftNode>();
            var context = new RaftServerContext();

            var handler = new CommandApplier(raftNode, context);

            // Act
            handler.Handle(@event);

            // Assert
            raftNode.Received().ApplyCommand();
        }
    }
}
