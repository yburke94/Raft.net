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
            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "FileDoesNotExist-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var bytes = BitConverter.GetBytes(100);

            var fileWriter = new BufferedSequentialFileWriter();

            Action actAction = () => fileWriter.Write(filePath, 0, bytes);

            // Act, Assert
            actAction.ShouldThrow<FileNotFoundException>("because the file needs to exist");
        }

        [Test]
        public void ThrowsIfOffsetPlusBytesToWriteExceedesFileLengthWhenWriteIsCalled()
        {
            // Arrange
            var bytes = BitConverter.GetBytes(100);
            var fileLength = bytes.Length + (bytes.Length/2);
            var offset = bytes.Length;
            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "UpdateFile-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var fileWriter = new BufferedSequentialFileWriter();
            fileWriter.CreateAndWrite(filePath, bytes, fileLength);

            Action actAction = () => fileWriter.Write(filePath, offset, bytes);

            // Act, Assert
            actAction.ShouldThrow<InvalidOperationException>(
                "because the file length would be exceeded with the write.");
        }

        [Test]
        public void WritesBytesAtGivenOffsetWhenWriteIsCalled()
        {
            // Arrange
            var bytes = BitConverter.GetBytes(100);
            var fileLength = bytes.Length * 2;
            var offset = bytes.Length;
            var filePath = Path.Combine(_testDumpsDir, string.Format(
                "UpdateFile-{0}.log", DateTime.Now.ToFileTimeUtc()));

            var fileWriter = new BufferedSequentialFileWriter();
            fileWriter.CreateAndWrite(filePath, bytes, fileLength);

            // Act
            fileWriter.Write(filePath, offset, bytes);

            // Assert
            File.ReadAllBytes(filePath)
                .Skip(offset)
                .SequenceEqual(bytes)
                .Should().BeTrue("because it should have written the specified bytes at that position.");
        }
    }
}
