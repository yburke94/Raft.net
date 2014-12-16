using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Infrastructure.IO;
using Raft.Server;
using Raft.Server.Configuration;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogWriterTests
    {
        private readonly string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "TestDumps\\Server\\Handlers\\IO");

        [Test]
        public void DoesNotHandleInternalCommands()
        {
            // Act, Assert
            typeof(ISkipInternalCommands).IsAssignableFrom(typeof(LogWriter))
                .Should().BeTrue();
        }

        [Test]
        public void CreateAndWriteNewJournalFileIfNoneExists()
        {
            // Arrange
            var filePath = Path.Combine(_logDir, "Journal.1");
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4*1024*1024;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(0);
            logMetadata.CurrentJournalOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            fileWriter.Received()
                .CreateAndWrite(filePath, data, fileLength);
        }

        [Test]
        public void IncrementJournalIdxInMetadataWhenFirstJournal()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(1);

            logMetadata.CurrentJournalIndex.Returns(0);
            logMetadata.CurrentJournalOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, BitConverter.GetBytes(34));

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            logMetadata.Received().IncrementJournalIndex();
        }

        [Test]
        public void CreateAndWriteNewJournalFileIfDataLengthExceedsFileLength()
        {
            // Arrange
            var filePath = Path.Combine(_logDir, "Journal.2");
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(2);
            logMetadata.CurrentJournalOffset.Returns(fileLength - (data.Length/2));

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            fileWriter.Received()
                .CreateAndWrite(filePath, data, fileLength);
        }

        [Test]
        public void AppendToExistingJournalFileIfDataLengthDoesNotExceedFileLength()
        {
            // Arrange
            var filePath = Path.Combine(_logDir, "Journal.1");
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;
            var offset = fileLength - data.Length;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.CurrentJournalOffset.Returns(offset);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            fileWriter.Received()
                .Write(filePath, offset, data);
        }

        [Test]
        public void ChangesFileOffsetInMetadataWhenWritingToFile()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.CurrentJournalOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            logMetadata.Received().SetJournalOffset(data.Length);
        }

        [Test]
        public void CallsLogMetadataFlushStrategyAfterEachWrite()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.CurrentJournalOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister, logMetadata, fileWriter, metadataFlushStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            metadataFlushStrategy.Received().FlushLogMetadata();
        }
    }
}
