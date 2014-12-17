using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using NSubstitute;
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

        [Test]
        public void ThrowsWhenPassedInViewAccessorSizeIsLessThanSizeofMetadataEntryStruct()
        {
            // Arrange
            var mmioViewAccessorSize = Marshal.SizeOf(typeof(LogMetadataEntry)) / 2;
            var mmioService = Substitute.For<IMmioService>();
            Action actAction = () => new LogMetadata(mmioService, mmioViewAccessorSize);

            // Act, Assert
            actAction.ShouldThrow<ArgumentException>(
                "because the viewAccessorSize must be greater than the size of the metadata entry type.");
        }

        [Test]
        public void ThrowsWhenPassedInViewAccessorSizeIsNotMultipleOfSizeofMetadataEntryStruct()
        {
            // Arrange
            var metadataEntrySize = Marshal.SizeOf(typeof(LogMetadataEntry));
            var mmioViewAccessorSize = (metadataEntrySize*13) - (metadataEntrySize/2);

            var mmioService = Substitute.For<IMmioService>();
            Action actAction = () => new LogMetadata(mmioService, mmioViewAccessorSize);

            // Act, Assert
            actAction.ShouldThrow<ArgumentException>(
                "because the viewAccessorSize must be a multiple of the size of the metadata entry type.");
        }

        [Test]
        public void InitialMetadataIsLoadedFromMemoryMappedFile()
        {
            // Arrange
            var mmioService = Substitute.For<IMmioService>();


            // Act

            // Assert
        }
    }
}
