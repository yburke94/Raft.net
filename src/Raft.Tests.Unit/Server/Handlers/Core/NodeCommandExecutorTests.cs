using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Enums;
using Raft.Infrastructure;
using Raft.Server.BufferEvents;
using Raft.Server.Handlers.Core;
using Serilog;

namespace Raft.Tests.Unit.Server.Handlers.Core
{
    [TestFixture]
    public class NodeCommandExecutorTests
    {
        [Test]
        public void CanFireCommandAtNodeStateMachine()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var node = new Node(Substitute.For<IEventDispatcher>());
            var handler = new NodeCommandExecutor(node, logger);
            var @event = new InternalCommandScheduled {Command = new JoinCluster()};
            node.CurrentState.Should().Be(NodeState.Initial);

            // Act
            handler.Handle(@event);

            // Assert
            node.CurrentState.Should().Be(NodeState.Follower);
        }

        [Test]
        public void CanInvokeNodeCommandHandler()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var node = new Node(Substitute.For<IEventDispatcher>());
            var handler = new NodeCommandExecutor(node, logger);

            node.FireAtStateMachine<JoinCluster>();

            node.CurrentState.Should().Be(NodeState.Follower);
            node.Properties.CurrentTerm.Should().Be(0L);

            var @event = new InternalCommandScheduled
            {
                Command = new SetNewTerm { Term = 3L }
            };

            // Act
            handler.Handle(@event);

            // Assert
            node.Properties.CurrentTerm.Should().Be(3L);
        }

        [Test]
        public void LogWarningWhenInternalCommandHasNoHandler()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            logger.ForContext(Arg.Any<string>(), Arg.Any<object>())
                .Returns(logger);

            var node = new Node(Substitute.For<IEventDispatcher>());

            var handler = new NodeCommandExecutor(node, logger);
            var @event = new InternalCommandScheduled
            {
                Command = new ThisCommandDoesNotExist()
            };

            // Act
            handler.Handle(@event);

            // Assert
            logger.Received(1).Warning(Arg.Any<string>(), Arg.Any<object[]>());
        }

        public class ThisCommandDoesNotExist : INodeCommand { }
    }
}
