using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure.IO;

namespace Raft.Tests.Unit.Infrastructure.IO
{
    [TestFixture]
    public class BufferedSequentialFileWriterTests
    {
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            
        }

        [Test]
        public void BufferedSequentialDiskWriterCreatesFileIfDoesNotExistAndOffsetIsZero()
        {
            // Arrange
            const int offset = 0;

            var filePath = Path.Combine(Path.GetTempPath(),
                string.Format("RAFTnet\\TestDumps\\BufferedDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter(bytes.Length);

            // Act
            fileWriter.Write(filePath, offset, bytes);

            // Assert
            File.Exists(filePath).Should().BeTrue();
        }

        [Test]
        public void BufferedSequentialDiskWriterCreatesFileWithCorrectLengthIfDoesNotExistAndOffsetIsZero()
        {
            // Arrange
            const int fileLength = 2 << 11;
            const int offset = 0;

            var filePath = Path.Combine(Path.GetTempPath(),
                string.Format("RAFTnet\\TestDumps\\BufferedDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter(fileLength);

            // Act
            fileWriter.Write(filePath, offset, bytes);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            new FileInfo(filePath).Length.Should().Be(fileLength);
        }

        [Test]
        public void BufferedSequentialDiskWriterThrowsIfFileDoesNotExistAndOffsetGreaterThanZero()
        {
            // Arrange
            
            // Act

            // Assert
        }

        [Test]
        public void BufferedSequentialDiskWriterThrowsIfLengthIsNotSet()
        {
            // Arrange

            // Act

            // Assert
        }

        [Test]
        public void BufferedSequentialDiskWriterThrowsIfOffsetPlusBytesToWriteIsGreaterThanLength()
        {
            // Arrange

            // Act

            // Assert
        }

        [Test]
        public void BufferedSequentialDiskWriterWritesBytes()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
