using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Server.Messages.AppendEntries;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogReplicatorTests
    {
        [Test]
        public void LogReplicatorSkipsInternalCommands()
        {
            // Act, Assert
            typeof(ISkipInternalCommands).IsAssignableFrom(typeof(LogReplicator))
                .Should().BeTrue();
        }

        [Test]
        public void LogReplicatorCallsAppendEntriesOnOtherNodesInCluster()
        {
            // Arrange
            var service1 = Substitute.For<IRaftService>();
            var service2 = Substitute.For<IRaftService>();

            var peers = new List<PeerNode> {
                new PeerNode {Channel = service1},
                new PeerNode {Channel = service2}
            };
            var logRegister = new LogRegister();
            var handler = new LogReplicator(peers, logRegister);

            var encodedLog = BitConverter.GetBytes(100);
            var @event = TestEventFactory.GetCommandEvent();
            logRegister.AddEncodedLog(@event.Id, encodedLog);

            // Act
            handler.Handle(@event);

            // Assert
            service1.Received().AppendEntries(Arg.Any<AppendEntriesRequest>());
            service2.Received().AppendEntries(Arg.Any<AppendEntriesRequest>());
        }

        [Test]
        public void LogReplicatorCallsAppendEntriesPassingEncodedLog()
        {
            // Arrange
            var encodedLog = BitConverter.GetBytes(100);
            AppendEntriesRequest request = null;

            var service = Substitute.For<IRaftService>();
            service.When(x => x.AppendEntries(Arg.Any<AppendEntriesRequest>()))
                .Do(x => request = x.Arg<AppendEntriesRequest>());

            var peers = new List<PeerNode> {
                new PeerNode {Channel = service}
            };
            var logRegister = new LogRegister();
            var handler = new LogReplicator(peers, logRegister);

            var @event = TestEventFactory.GetCommandEvent();
            logRegister.AddEncodedLog(@event.Id, encodedLog);

            // Act
            handler.Handle(@event);

            // Assert
            request.Should().NotBeNull();
            request.Entries.Should().HaveCount(1);
            request.Entries[0].Should().Equal(encodedLog);
        }

    }
}
