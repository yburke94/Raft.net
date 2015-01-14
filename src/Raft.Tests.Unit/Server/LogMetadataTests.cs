using System;
using FluentAssertions;
using NUnit.Framework;
using Raft.Server;

namespace Raft.Tests.Unit.Server
{
    [TestFixture]
    public class LogMetadataTests
    {
        [Test]
        public void CanIncrementJournalIndexInLogMetadata()
        {
            // Arrange
            var logMetadata = new LogMetadata(0, 0);

            // Act
            logMetadata.IncrementJournalIndex();

            // Assert
            logMetadata.CurrentJournalIndex.Should().Be(1);
        }

        [Test]
        public void CanSetNextJournalEntryOffsetWhenAddingLogEntry()
        {
            // Arrange
            const long  currOffset = 10;
            var logMetadata = new LogMetadata(0, currOffset);
            var dataLength = new Random().Next();


            // Act
            logMetadata.AddLogEntryToIndex(0, dataLength);

            // Assert
            logMetadata.NextJournalEntryOffset.Should().Be(currOffset + dataLength);
        }

        [Test]
        public void ShouldSetNextJournalEntryOffsetToZeroWhenIncrementingJournalIndex()
        {
            // Arrange
            var logMetadata = new LogMetadata(0, 1024);

            // Act
            logMetadata.IncrementJournalIndex();

            // Assert
            logMetadata.CurrentJournalIndex.Should().Be(1);
            logMetadata.NextJournalEntryOffset.Should().Be(0);
        }
    }
}
