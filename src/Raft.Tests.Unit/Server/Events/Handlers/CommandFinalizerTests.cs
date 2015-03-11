using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Core.StateMachine;
using Raft.Server.Events.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Events.Handlers
{
    [TestFixture]
    public class CommandFinalizerTests
    {

        [Test]
        public void DoesCommitLogEntryOnRaftNode()
        {
            // Arrange
            const long commitIdx = 3L;
            const long term = 5L;

            var @event = TestEventFactory.GetCommandEvent(commitIdx, new byte[8]);
            @event.LogEntry.Term = term;

            var node = Substitute.For<IRaftNode>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(node, serviceLocator);

            // Act
            handler.Handle(@event);

            // Assert
            node.Received().CommitLogEntry(Arg.Is(commitIdx), Arg.Is(term));
        }

        [Test]
        public void DoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);

            var raftNode = Substitute.For<IRaftNode>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(raftNode, serviceLocator);

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
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);
            var raftNode = Substitute.For<IRaftNode>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(raftNode, serviceLocator);

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
                1L, new byte[4],
                () => shouldEqualTrueWhenCommandExecutes = true);

            var raftNode = Substitute.For<IRaftNode>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(raftNode, serviceLocator);

            // Act
            handler.Handle(@event);

            // Assert
            shouldEqualTrueWhenCommandExecutes
                .Should().BeTrue();
        }

        [Test]
        public void DoesCallApplyCommandOnRaftNodeWithCorrectLogIdx()
        {
            // Arrange
            const long logIdx = 3L;
            var @event = TestEventFactory.GetCommandEvent(logIdx, new byte[8]);

            var raftNode = Substitute.For<IRaftNode>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(raftNode, serviceLocator);

            // Act
            handler.Handle(@event);

            // Assert
            raftNode.Received().ApplyCommand(Arg.Any<long>());
        }
    }
}
