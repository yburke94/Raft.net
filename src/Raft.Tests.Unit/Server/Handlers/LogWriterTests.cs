using System;
using System.Linq;
using System.Linq.Expressions;
using NSubstitute;
using NUnit.Framework;
using Raft.Infrastructure.Journaler;
using Raft.Server.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogWriterTests
    {
        [Test]
        public void WritesBlockToJournaler()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);

            var @event = TestEventFactory.GetCommandEvent(1L, data);
            var journaler = Substitute.For<IJournal>();

            var handler = new LogWriter(journaler);

            Expression<Predicate<byte[]>> match = x => x.SequenceEqual(data);

            // Act
            handler.OnNext(@event, 0, true);

            // Assert
            journaler.Received().WriteBlock(Arg.Is(match));
        }
    }
}
