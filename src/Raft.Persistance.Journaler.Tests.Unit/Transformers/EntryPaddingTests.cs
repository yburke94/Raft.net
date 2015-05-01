using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Persistance.Journaler.Kernel;
using Raft.Persistance.Journaler.Transformers;

namespace Raft.Persistance.Journaler.Tests.Unit.Transformers
{
    [TestFixture]
    public class EntryPaddingTests
    {
        [Test]
        public void AppendsZeroAsPaddedBytesLengthWhenBufferedIoAndDoesNotAlignToSectorSize()
        {
            // Arrange
            var sectorSize = SectorSize.Get(AppDomain.CurrentDomain.BaseDirectory);

            var entry = new byte[sectorSize / 2];
            for (var i = 0; i < entry.Length; i++)
                entry[i] = 1;

            var configuration = new JournalConfiguration(
                AppDomain.CurrentDomain.BaseDirectory, string.Empty, 0L, IoType.Buffered);

            var entryPadding = new EntryPadding(configuration);

            // Act
            var result = entryPadding.Transform(entry, new Dictionary<string, string>());

            // Assert
            result.Length.Should().Be(entry.Length+sizeof(int));

            for (var i = entry.Length; i < entry.Length + sizeof(int); i++)
                result[i].Should().Be(0);
        }

        [Test]
        public void AppendsPaddedBytesLengthToEntryBytesAndAlignesToSectorSize()
        {
            // Arrange
            var sectorSize = SectorSize.Get(AppDomain.CurrentDomain.BaseDirectory);
            var expectedPaddingLengthInBytes = BitConverter.GetBytes((sectorSize / 2)-sizeof(int)/*Padding length bytes*/);

            var entry = new byte[sectorSize / 2];
            for (var i = 0; i < entry.Length; i++)
                entry[i] = 1;

            var configuration = new JournalConfiguration(
                AppDomain.CurrentDomain.BaseDirectory, string.Empty, 0L, IoType.Unbuffered);

            var entryPadding = new EntryPadding(configuration);

            // Act
            var result = entryPadding.Transform(entry, new Dictionary<string, string>());

            // Assert
            result.Length.Should().Be((int)sectorSize);

            for (var i = entry.Length; i < entry.Length + sizeof(int); i++)
                result[i].Should().Be(expectedPaddingLengthInBytes[i - entry.Length]);
        }

        [Test]
        public void AppendsZeroAsPaddedBytesLengthToEntryBytesWhenNoPaddingIsRequiredToAlignToSectorSize()
        {
            // Arrange
            var sectorSize = SectorSize.Get(AppDomain.CurrentDomain.BaseDirectory);

            var entry = new byte[(sectorSize * 2) - sizeof(int)/*Padding length bytes*/];
            for (var i = 0; i < entry.Length; i++)
                entry[i] = 1;

            var configuration = new JournalConfiguration(
                AppDomain.CurrentDomain.BaseDirectory, string.Empty, 0L, IoType.Unbuffered);

            var entryPadding = new EntryPadding(configuration);

            // Act
            var result = entryPadding.Transform(entry, new Dictionary<string, string>());

            // Assert
            result.Length.Should().Be((int)sectorSize*2);

            for (var i = entry.Length; i < entry.Length + sizeof(int); i++)
                result[i].Should().Be(0);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AppendsPaddedBytesToEntryBytesAfterLength(int sectorSizeDivide)
        {
            // Arrange
            var sectorSize = SectorSize.Get(AppDomain.CurrentDomain.BaseDirectory);

            var entry = new byte[(sectorSize/sectorSizeDivide)];
            for (var i = 0; i < sectorSize/sectorSizeDivide; i++)
                entry[i] = 1;

            var configuration = new JournalConfiguration(
                AppDomain.CurrentDomain.BaseDirectory, string.Empty, 0L, IoType.Unbuffered);

            var entryPadding = new EntryPadding(configuration);

            // Act
            var result = entryPadding.Transform(entry, new Dictionary<string, string>());

            // Assert
            result.Length.Should().Be((int) sectorSize);

            var bytesPriorToLength = result.Take(entry.Length).ToArray();
            for (var i = 0; i < entry.Length; i++)
                bytesPriorToLength[i].Should().Be(1);

            var bytesAfterLength = result.Skip(entry.Length + sizeof(int)/*Padding length bytes*/).ToArray();
            for (var i = 0; i < bytesAfterLength.Length; i++)
                bytesAfterLength[i].Should().Be(0);
        }
    }
}
