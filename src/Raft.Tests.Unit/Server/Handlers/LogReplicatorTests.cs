using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Cluster;
using Raft.Server.Handlers.Leader;
using Raft.Service.Contracts;
using Raft.Service.Contracts.Messages.AppendEntries;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogReplicatorTests
    {
        [Test, Ignore("Change when there is a way to retieve the channel and stop ignoring")]
        public void LogReplicatorCallsAppendEntriesOnOtherNodesInCluster()
        {
            // Arrange
            var service1 = Substitute.For<IRaftService>();
            var service2 = Substitute.For<IRaftService>();

            var peers = new List<PeerNode> {
                new PeerNode(),
                new PeerNode()
            };

            var handler = new LogReplicator(peers);

            var encodedLog = BitConverter.GetBytes(100);
            var @event = TestEventFactory.GetCommandEvent(1L, encodedLog);

            // Act
            handler.Handle(@event);

            // Assert
            service1.Received().AppendEntries(Arg.Any<AppendEntriesRequest>());
            service2.Received().AppendEntries(Arg.Any<AppendEntriesRequest>());
        }

        [Test, Ignore("Change when there is a way to retieve the channel and stop ignoring")]
        public void LogReplicatorCallsAppendEntriesPassingEncodedLog()
        {
            // Arrange
            var encodedLog = BitConverter.GetBytes(100);
            AppendEntriesRequest request = null;

            var service = Substitute.For<IRaftService>();
            service.When(x => x.AppendEntries(Arg.Any<AppendEntriesRequest>()))
                .Do(x => request = x.Arg<AppendEntriesRequest>());

            var peers = new List<PeerNode> {
                new PeerNode()
            };

            var handler = new LogReplicator(peers);

            var @event = TestEventFactory.GetCommandEvent(1L, encodedLog);

            // Act
            handler.Handle(@event);

            // Assert
            request.Should().NotBeNull();
            request.Entries.Should().HaveCount(1);
            request.Entries[0].Should().Equal(encodedLog);
        }

    }
}
