using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
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
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(0);
            logMetadata.NextJournalEntryOffset.Returns(0);
            logMetadata.When(x => x.IncrementJournalIndex())
                .Do(_ => logMetadata.CurrentJournalIndex.Returns(1));

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

            // Act
            handler.Handle(@event);

            // Assert+
            fileWriter.Received()
                .CreateAndWrite(filePath, data, fileLength);
        }

        [Test]
        public void IncrementJournalIdxInMetadataWhenFirstJournal()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(1);

            logMetadata.CurrentJournalIndex.Returns(0);
            logMetadata.NextJournalEntryOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, BitConverter.GetBytes(34));

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

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
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.NextJournalEntryOffset.Returns(fileLength - (data.Length/2));

            logMetadata.When(x => x.IncrementJournalIndex())
                .Do(_ => logMetadata.CurrentJournalIndex.Returns(2));

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

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
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.NextJournalEntryOffset.Returns(offset);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            fileWriter.Received()
                .Write(filePath, offset, data);
        }

        [Test]
        public void AddsLogEntryIndexAndDataLengthToMetadataWhenWritingToFile()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;
            const long logIdx = 30;
            const long nextOffset = 30;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.NextJournalEntryOffset.Returns(0);

            raftNode.LastLogIndex.Returns(logIdx - 1);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            logMetadata.Received().AddLogEntryToIndex(logIdx, data.Length);
        }

        [Test]
        public void CallsLogMetadataFlushStrategyAfterEachWrite()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);
            const long fileLength = 4 * 1024 * 1024;

            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var raftNode = Substitute.For<IRaftNode>();
            var fileWriter = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var logMetadata = Substitute.For<ILogMetadata>();
            var metadataFlushStrategy = Substitute.For<IMetadataFlushStrategy>();

            raftConfiguration.LogDirectory.Returns(_logDir);
            raftConfiguration.JournalFileName.Returns("Journal");
            raftConfiguration.JournalFileLength.Returns(fileLength);

            logMetadata.CurrentJournalIndex.Returns(1);
            logMetadata.NextJournalEntryOffset.Returns(0);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister,
                logMetadata, fileWriter, metadataFlushStrategy, raftNode);

            // Act
            handler.Handle(@event);

            // Assert
            metadataFlushStrategy.Received().FlushLogMetadata();
        }
    }
}
