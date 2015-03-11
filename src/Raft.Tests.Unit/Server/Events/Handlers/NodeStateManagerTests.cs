using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Core.StateMachine;
using Raft.Server.Events.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Events.Handlers
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
            raftNode.Received().CreateCluster();
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
            raftNode.Received().ScheduleCommandExecution();
        }
    }
}
