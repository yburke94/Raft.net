using System;
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
    public class CommandFinalizerTests
    {
        [Test]
        public void CommandFinalizerDoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();
            var handler = new CommandFinalizer(new LogRegister(), raftNode);

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
            var handler = new CommandFinalizer(new LogRegister(), raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            @event.TaskCompletionSource.Task.Result
                .Should().NotBeNull();

            @event.TaskCompletionSource.Task.Result.Successful
                .Should().BeTrue();
        }

        [Test]
        public void CommandFinalizerDoesRemoveEntriesInLogRegistryForEvent()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            logRegister.AddEncodedLog(@event.Id, BitConverter.GetBytes(100));

            var handler = new CommandFinalizer(logRegister, raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            logRegister.HasLogEntry(@event.Id)
                .Should().BeFalse();
        }

        [Test]
        public void CommandFinalizerDoesCallEntryLoggedOnRaftNode()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = TestEventFactory.GetCommandEvent();

            var handler = new CommandFinalizer(new LogRegister(), raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            raftNode.Received().EntryLogged();
        }
    }
}
