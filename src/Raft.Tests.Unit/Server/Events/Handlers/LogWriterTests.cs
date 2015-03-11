using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Journaler;
using Raft.Server.Events.Handlers;
using Raft.Server.Events.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Events.Handlers
{
    [TestFixture]
    public class LogWriterTests
    {
        [Test]
        public void DoesNotHandleInternalCommands()
        {
            // Act, Assert
            typeof(ISkipInternalCommands).IsAssignableFrom(typeof(LogWriter))
                .Should().BeTrue();
        }

        [Test]
        public void WritesBlockToJournaler()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);

            var @event = TestEventFactory.GetCommandEvent(1L, data);
            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var handler = new LogWriter(journaler, node);

            Expression<Predicate<byte[]>> match = x => x.SequenceEqual(data);

            // Act
            handler.OnNext(@event, 0, true);

            // Assert
            journaler.Received().WriteBlock(Arg.Is(match));
        }
    }
}
