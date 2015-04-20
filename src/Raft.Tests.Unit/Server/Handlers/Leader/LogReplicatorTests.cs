using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Cluster;
using Raft.Server.Handlers.Leader;
using Raft.Service.Contracts;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers.Leader
{
    [TestFixture]
    public class LogReplicatorTests
    {
        [Test]
        public void GetsAllPeersToReplicateTo()
        {
            // Arrange
            var peerService = Substitute.For<IInternalPeerService>();
            var handler = new LogReplicator(peerService);

            // Act
            handler.Handle(TestEventFactory.GetCommandEvent());

            // Assert
            peerService.Received().GetPeersInCluster();
        }

        [Test]
        public void ScheduleATaskForEachPeerToReplicateTo()
        {
            // Arrange
            var peerService = Substitute.For<IInternalPeerService>();
            peerService.GetPeersInCluster()
                .Returns(_ => new List<Peer>
                {
                    new Peer(),
                    new Peer()
                });

            var scheduler = new TestTaskScheduler();

            var handler = new LogReplicator(peerService);

            // Act
            handler.Handle(TestEventFactory.GetCommandEvent());

            // Assert
            peerService.Received().GetPeersInCluster();
            scheduler.TaskQueue.Count.Should().Be(2);
        }

        [Test]
        public void ScheduledTaskReceivesAReplicationRequestForTheSpecificPeer()
        {
            // Arrange
            var peerList = new List<Peer>
            {
                new Peer {NodeId = Guid.NewGuid()},
                new Peer {NodeId = Guid.NewGuid()}
            };

            var peerService = Substitute.For<IInternalPeerService>();
            peerService.GetPeersInCluster().Returns(_ => peerList);

            var scheduler = new TestTaskScheduler();

            var handler = new LogReplicator(peerService);

            // Act
            handler.Handle(TestEventFactory.GetCommandEvent());

            // Assert
            for (var i = 0; i < scheduler.TaskQueue.Count; i++)
            {
                var replicationResult = ((Task<LogReplicator.ReplicationResult>)scheduler.TaskQueue[i]).Result;
                replicationResult.NodeId.Should().Be(peerList[i].NodeId);
            }
        }
    }
}
