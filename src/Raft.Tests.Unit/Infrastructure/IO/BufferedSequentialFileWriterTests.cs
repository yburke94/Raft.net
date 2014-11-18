using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure.IO;

namespace Raft.Tests.Unit.Infrastructure.IO
{
    [TestFixture]
    public class BufferedSequentialFileWriterTests
    {
        private readonly string _testDumpsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "TestDumps\\Infrastructure\\IO");

        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Directory.Exists(_testDumpsDir))
                Directory.CreateDirectory(_testDumpsDir);
        }

        [Test]
        public void ThrowsWhenCallingCreateAndWriteAndFileExists()
        {
            // Arrange
            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "FileAlreadyExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            File.Create(filePath);

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter();

            Action actAction = () => fileWriter.CreateAndWrite(filePath, bytes, bytes.Length);

            // Act, Assert
            actAction.ShouldThrow<InvalidOperationException>("becasue the file already exists");
        }

        [Test]
        public void CreatesFileWhenCallingCreateAndWrite()
        {
            // Arrange
            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "FileDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter();

            // Act
            fileWriter.CreateAndWrite(filePath, bytes, bytes.Length);

            // Assert
            File.Exists(filePath).Should().BeTrue("because calling CreateAndWrite() creates the file.");
        }

        [Test]
        public void CreatesFileAndSetsLengthWhenCallingCreateAndWrite()
        {
            // Arrange
            const int fileLength = 2 << 13;

            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "FileDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter();

            // Act
            fileWriter.CreateAndWrite(filePath, bytes, fileLength);

            // Assert
            File.Exists(filePath)
                .Should().BeTrue("because calling CreateAndWrite() creates the file.");

            new FileInfo(filePath).Length
                .Should().Be(fileLength, "because the method should preset the length of the file.");
        }

        [Test]
        public void CreatesFileAndWritesDataWhenCallingCreateAndWrite()
        {
            // Arrange
            const int fileLength = 2 << 13;

            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "FileDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter();

            // Act
            fileWriter.CreateAndWrite(filePath, bytes, fileLength);

            // Assert
            File.Exists(filePath)
                .Should().BeTrue("because calling CreateAndWrite() creates the file.");

            File.ReadAllBytes(filePath)
                .Take(bytes.Length)
                .SequenceEqual(bytes)
                .Should().BeTrue("because the bytes should have been written to the beggining of the file.");
        }

        [Test]
        public void ThrowsIfFileDoesNotExistWhenWriteIsCalled()
        {
            // Arrange
            
            // Act

            // Assert
        }

        [Test]
        public void ThrowsIfOffsetPlusBytesToWriteExceedesFileLengthWhenWriteIsCalled()
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
