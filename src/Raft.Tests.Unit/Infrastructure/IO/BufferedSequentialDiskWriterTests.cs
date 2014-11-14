using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure.IO;

namespace Raft.Tests.Unit.Infrastructure.IO
{
    [TestFixture]
    public class BufferedSequentialDiskWriterTests
    {
        private byte[] _bytesToWrite;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            // 1MB
            const int fileSize = 2 << 19;
            var bytes = new byte[fileSize];

            for (var i = 0; i < fileSize; i++)
                bytes[i] = (byte) (i%byte.MaxValue);

            _bytesToWrite = bytes;
        }

        [Test]
        public void BufferedSequentialDiskWriterCreatesFileIfDoesNotExist()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDump\\BufWriterTest1.bin");
            if (File.Exists(filePath))
                File.Delete(filePath);

            var diskWriter = new BufferedSequentialDiskWriter();

            // Act
            diskWriter.Write(filePath, _bytesToWrite);

            // Assert
            File.Exists(filePath).Should().BeTrue();
        }

        [Test]
        public void BufferedSequentialDiskWriterAppendsToFileIfExists()
        {
            // Arrange

            // Act

            // Assert
        }

        [Test]
        public void BufferedSequentialDiskWriterReturnsBytesWritten()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
