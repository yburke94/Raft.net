﻿using System.Linq;
using System.Threading.Tasks;
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
            var @event = new CommandScheduledEvent()
                .ResetEvent(new TestInternalCommand(), new TaskCompletionSource<LogResult>());

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
            var @event = new CommandScheduledEvent()
                .ResetEvent(new TestInternalCommand(), new TaskCompletionSource<LogResult>());

            var handler = new NodeStateValidator(raftNode);

            // Act
            handler.OnNext(@event, 0, false);

            // Assert
            raftNode.Received().LogEntry();
        }
    }
}