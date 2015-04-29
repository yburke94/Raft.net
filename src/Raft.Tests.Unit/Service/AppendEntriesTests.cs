using System;
using System.ServiceModel;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;
using Raft.Core.Timer;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Service;
using Raft.Service.Contracts;
using Raft.Tests.Unit.TestHelpers;
using Serilog;

namespace Raft.Tests.Unit.Service
{
    [TestFixture]
    public class AppendEntriesTests
    {
        [Test]
        public void AppendEntriesResetsTimer()
        {
            // Arrange
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);
            raftNode.Properties.Returns(new NodeProperties());
            raftNode.Log.Returns(new InMemoryLog());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            service.AppendEntries(message);

            // Assert
            timer.Received(1).ResetTimer();
        }

        [Test]
        public void AppendEntriesReturnsNodesCurrentTerm()
        {
            // Arrange
            const int expectedTerm = 456;
            var message = new AppendEntriesRequest();

            var raftNode = Substitute.For<INode>();
            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            raftNode.Properties.Returns(new NodeProperties {
                CurrentTerm = expectedTerm
            });
            raftNode.Log.Returns(new InMemoryLog());

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Term.ShouldBeEquivalentTo(expectedTerm);
        }

        [Test]
        public void AppendEntriesIsUnsuccessfulIfLeaderTermIsLessThanCurrentTerm()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 234
            };

            var nodeData = new NodeProperties
            {
                CurrentTerm = message.Term + 10,
            };

            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(nodeData);
            raftNode.Log.Returns(new InMemoryLog());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Success.Should().BeFalse(
                "because node cannot apply log " +
                "if the leader term is less than the nodes term");
        }

        [Test]
        public void AppendEntriesReturnsFalseIfPrevEntryTermDoesNotMatch()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                PreviousLogIndex = 0,
                PreviousLogTerm = 1
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            var raftLog = new InMemoryLog();
            raftLog.SetLogEntry(1, 2L);

            raftNode.Properties.Returns(new NodeProperties());
            raftNode.Log.Returns(new InMemoryLog());

            // Act
            var response = service.AppendEntries(message);

            // Assert
            response.Success.Should().BeFalse(
                "because node cannot apply log if the term " +
                "for the leaders previous log index does not match.");
        }

        [Test]
        public void AppendEntriesAmendsTermOnRaftNodeWhenTermIsGreaterThanCurrentTerm()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 1,
                PreviousLogIndex = 1,
                PreviousLogTerm = 0
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            var nodeData = new NodeProperties();
            var nodeLog = new InMemoryLog();

            nodeLog.SetLogEntry(1, 0);
            raftNode.Properties.Returns(nodeData);
            raftNode.Log.Returns(nodeLog);

            // Act
            service.AppendEntries(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(2);
            nodePublisher.Events[0].Command.Should().BeOfType<SetNewTerm>();
        }

        [Test]
        public void CancelElectionIfCandidateAndReceiveAppendEntries()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 0,
                PreviousLogIndex = 0,
                PreviousLogTerm = 0
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Candidate);
            raftNode.Properties.Returns(new NodeProperties());
            raftNode.Log.Returns(new InMemoryLog());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            nodePublisher.OnPublish(() => raftNode.CurrentState.Returns(NodeState.Follower));

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            service.AppendEntries(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(2);
            nodePublisher.Events[0].Command.Should().BeOfType<CancelElection>();
        }

        [Test]
        public void SetLeaderIdWhenFollowerAndAppendEntriesRecieved()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 0,
                PreviousLogIndex = 0,
                PreviousLogTerm = 0
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);
            raftNode.Properties.Returns(new NodeProperties());
            raftNode.Log.Returns(new InMemoryLog());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            service.AppendEntries(message);

            // Assert
            nodePublisher.Events.Should().HaveCount(1);
            nodePublisher.Events[0].Command.Should().BeOfType<SetLeaderInformation>();
        }

        [Test]
        public void ThrowsWhenAppendEntriesReceivedAndNodeWillNotBeTransitionedToFollower()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 24,
                PreviousLogIndex = 0,
                PreviousLogTerm = 0
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Leader);
            raftNode.Properties.Returns(new NodeProperties {CurrentTerm = 24});

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            var actAction = new Action(() => service.AppendEntries(message));

            // Assert
            actAction.ShouldThrow<FaultException<MultipleLeadersForTermFault>>();
        }

        [Test]
        public void PublishesToBufferAfterSafetyChecks()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 11,
                PreviousLogIndex = 2,
                PreviousLogTerm = 13,
                LeaderCommit = 12,
                Entries = new []
                {
                    BitConverter.GetBytes(100),
                    BitConverter.GetBytes(200)
                }
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);

            var nodeData = new NodeProperties();
            var nodeLog = new InMemoryLog();

            nodeLog.SetLogEntry(message.PreviousLogIndex, message.PreviousLogTerm);

            raftNode.Properties.Returns(nodeData);
            raftNode.Log.Returns(nodeLog);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            service.AppendEntries(message);

            // Assert
            appendEntriesPublisher.Received()
                .PublishEvent(Arg.Is<AppendEntriesRequested>(requested =>
                    requested.PreviousLogIndex == message.PreviousLogIndex &&
                    requested.PreviousLogTerm == message.PreviousLogTerm &&
                    requested.LeaderCommit == message.LeaderCommit &&
                    requested.Entries == message.Entries));
        }

        [Test]
        public void LogsWarningWhenRequestTermIsLessThanCurrentTerm()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                Term = 234
            };

            var nodeData = new NodeProperties
            {
                CurrentTerm = message.Term + 10,
            };

            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(nodeData);
            raftNode.Log.Returns(new InMemoryLog());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            // Act
            service.AppendEntries(message);

            // Assert
            logger.Received().Warning(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void LogsInfoWhenRequestIsReturningFalseDueToLogMismatch()
        {
            // Arrange
            var message = new AppendEntriesRequest
            {
                PreviousLogIndex = 0,
                PreviousLogTerm = 1
            };

            var raftNode = Substitute.For<INode>();
            raftNode.CurrentState.Returns(NodeState.Follower);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var logger = Substitute.For<ILogger>();
            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode, logger);

            var raftLog = new InMemoryLog();
            raftLog.SetLogEntry(1, 2L);

            raftNode.Properties.Returns(new NodeProperties());
            raftNode.Log.Returns(new InMemoryLog());

            // Act
            service.AppendEntries(message);

            // Assert
            logger.Received().Information(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
