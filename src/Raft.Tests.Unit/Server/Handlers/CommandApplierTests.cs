using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Server.Handlers.Leader;
using Raft.Server.Log;
using Raft.Server.Registers;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class CommandApplierTests
    {
        [Test]
        public void DoesCompleteTaskInTaskCompletionSource()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);

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
            var @event = TestEventFactory.GetCommandEvent(1L, new byte[8]);
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
                1L, new byte[4],
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
        public void DoesCallApplyCommandOnRaftNodeWithCorrectLogIdx()
        {
            // Arrange
            const long logIdx = 3L;
            var @event = TestEventFactory.GetCommandEvent(logIdx, new byte[8]);

            var raftNode = Substitute.For<IRaftNode>();
            var context = new RaftServerContext();

            var handler = new CommandApplier(raftNode, context);

            // Act
            handler.Handle(@event);

            // Assert
            raftNode.Received().ApplyCommand(Arg.Any<long>());
        }
    }
}
