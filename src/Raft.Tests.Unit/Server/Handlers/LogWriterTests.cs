using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers
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

        [Test]
        public void CallsAddLogEntryOnRaftNodeForAllEntriesWrittenToLog()
        {
            // Arrange
            const long commitIdx = 3L;
            var data = BitConverter.GetBytes(1);
            var @event = TestEventFactory.GetCommandEvent(commitIdx, data);

            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var handler = new LogWriter(journaler, node);

            // Act
            handler.Handle(@event);

            // Assert
            node.Received().CommitLogEntry(Arg.Is(commitIdx));
        }
    }
}
