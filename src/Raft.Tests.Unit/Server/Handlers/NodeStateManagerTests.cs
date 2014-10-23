using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class NodeStateManagerTests
    {
        [Test]
        public void CanExecuteInternalCommandStateMachineAction()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = new CommandScheduledEvent {
                Command = new TestInternalCommand(),
                WhenLogged = _ => { }
            };

            var handler = new NodeStateValidator(raftNode);

            // Act
            handler.OnNext(@event, 0, false);

            // Assert
            raftNode.Received().JoinCluster();
        }

        public void LogsEntryWhenHandlingRaftCommand()
        {
            // Arrange
            var raftNode = Substitute.For<IRaftNode>();
            var @event = new CommandScheduledEvent {
                Command = new TestCommand(),
                WhenLogged = _ => { }
            };
            var handler = new NodeStateValidator(raftNode);

            // Act
            handler.OnNext(@event, 0, false);

            // Assert
            raftNode.Received().LogEntry();
        }
    }
}
