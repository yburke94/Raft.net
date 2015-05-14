using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers.Leader
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

            var @event = TestEventFactory.GetCommandEvent(commitIdx, BitConverter.GetBytes(199));
            @event.LogEntry.Term = term;

            var serviceLocator = Substitute.For<IServiceLocator>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            nodePublisher.Events.Count.Should().BeGreaterThan(0);
            nodePublisher.Events[0].Command.Should().BeOfType<CommitEntry>();
            ((CommitEntry) nodePublisher.Events[0].Command).EntryIdx.Should().Be(commitIdx);
            ((CommitEntry)nodePublisher.Events[0].Command).EntryTerm.Should().Be(term);
            ((CommitEntry) nodePublisher.Events[0].Command).Entry.SequenceEqual(@event.EncodedEntry).Should().BeTrue();
        }

        [Test]
        public void DoesCompleteTaskSuccessfullyInTaskCompletionSource()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);

            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var serviceLocator = Substitute.For<IServiceLocator>();

            var handler = new CommandFinalizer(serviceLocator, nodePublisher);

            // Act
            handler.Handle(@event);

            // Assert
            @event.HasCompletedSuccessfully().Should().BeTrue();
        }

        [Test]
        public void DoesExecuteCommand()
        {
            // Arrange
            var shouldEqualTrueWhenCommandExecutes = false;

            var @event = TestEventFactory.GetCommandEvent(
                1L, new byte[4],
                () => shouldEqualTrueWhenCommandExecutes = true);

            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
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

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
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
