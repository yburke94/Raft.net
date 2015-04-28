using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Contracts.Persistance;
using Raft.Server.Data;
using Raft.Server.Handlers.Leader;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers.Leader
{
    [TestFixture]
    public class LogWriterTests
    {
        [Test]
        public void ThrowsIfLogEntryNotSetIsNull()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            // Act
            Action actAction = () =>
                new LogWriter(Substitute.For<IWriteDataBlocks>())
                    .Handle(@event);

            // Assert
            actAction.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void SetsMetadataOnBlockWritten()
        {
            // Arrange
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();

            var data = BitConverter.GetBytes(1);
            var @event = TestEventFactory.GetCommandEvent(1L, data);

            Expression<Predicate<DataBlock>> match = x =>
                x.Metadata.ContainsKey("BodyType") &&
                x.Metadata["BodyType"].Equals(typeof(LogEntry).AssemblyQualifiedName);

            var handler = new LogWriter(writeDataBlocks);

            // Act
            handler.OnNext(@event, 0, true);

            // Assert
            writeDataBlocks.Received().WriteBlock(Arg.Is(match));
        }

        [Test]
        public void WritesBlockToJournaler()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);

            var @event = TestEventFactory.GetCommandEvent(1L, data);
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();

            var handler = new LogWriter(writeDataBlocks);

            Expression<Predicate<DataBlock>> match = x => x.Data.SequenceEqual(data);

            // Act
            handler.OnNext(@event, 0, true);

            // Assert
            writeDataBlocks.Received().WriteBlock(Arg.Is(match));
        }
    }
}
