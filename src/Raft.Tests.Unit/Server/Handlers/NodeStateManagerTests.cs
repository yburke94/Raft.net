using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
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
            var @event = TestEventFactory.GetInternalCommandEvent();

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
            var @event = TestEventFactory.GetCommandEvent();

            var handler = new NodeStateValidator(raftNode);

            // Act
            handler.OnNext(@event, 0, false);

            // Assert
            raftNode.Received().LogEntry();
        }
    }
}
