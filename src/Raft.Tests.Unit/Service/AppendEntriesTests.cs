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
using Raft.Server.Data;
using Raft.Service;
using Raft.Service.Contracts;
using Raft.Tests.Unit.TestHelpers;

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
            raftNode.Data.Returns(new NodeData { Log = new NodeLog() });

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

            raftNode.Data.Returns(new NodeData
            {
                Log = new NodeLog(),
                CurrentTerm = expectedTerm
            });

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

            var nodeData = new NodeData
            {
                CurrentTerm = message.Term + 10,
                Log = new NodeLog()
            };

            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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
            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

            var raftLog = new NodeLog();
            raftLog.SetLogEntry(1, 2L);

            raftNode.Data.Returns(new NodeData { Log = raftLog });

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
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

            var nodeData = new NodeData
            {
                Log = new NodeLog()
            };
            
            nodeData.Log.SetLogEntry(1, 0);
            raftNode.Data.Returns(nodeData);

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
            raftNode.Data.Returns(new NodeData());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();
            nodePublisher.OnPublish(() => raftNode.CurrentState.Returns(NodeState.Follower));

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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
            raftNode.Data.Returns(new NodeData());

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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
            raftNode.Data.Returns(new NodeData {CurrentTerm = 24});

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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

            var nodeData = new NodeData {Log = new NodeLog()};
            nodeData.Log.SetLogEntry(message.PreviousLogIndex, message.PreviousLogTerm);

            raftNode.Data.Returns(nodeData);

            var timer = Substitute.For<INodeTimer>();
            var appendEntriesPublisher = Substitute.For<IPublishToBuffer<AppendEntriesRequested>>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var service = new RaftService(appendEntriesPublisher, nodePublisher, timer, raftNode);

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
    }
}
