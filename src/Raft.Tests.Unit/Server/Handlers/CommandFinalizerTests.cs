using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Server.Handlers.Core;
using Raft.Server.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers
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

            var serviceLocator = Substitute.For<IServiceLocator>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            nodePublisher.Events.Count.Should().BeGreaterThan(0);
            nodePublisher.Events[0].Command.Should().BeOfType<CommitEntry>();
            ((CommitEntry) nodePublisher.Events[0].Command).EntryIdx.Should().Be(commitIdx);
            ((CommitEntry)nodePublisher.Events[0].Command).EntryTerm.Should().Be(term);
        }

        [Test]
        public void DoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            @event.CompletionSource.Task.IsCompleted
                .Should().BeTrue();
        }

        [Test]
        public void DoesSetTaskResultWithSuccessfulLogResult()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            @event.CompletionSource.Task.Result
                .Should().NotBeNull();

            @event.CompletionSource.Task.Result.Successful
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

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

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

            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            nodePublisher.Events.Count.Should().BeGreaterThan(1);
            nodePublisher.Events[1].Command.Should().BeOfType<ApplyEntry>();
            ((ApplyEntry)nodePublisher.Events[1].Command).EntryIdx.Should().Be(logIdx);
        }
    }
}
