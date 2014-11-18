using System;
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
        [Test]
        public void LogWriterDoesNotHandleInternalCommands()
        {
            // Act, Assert
            new LogWriter(null, null, null, null).SkipInternalCommands
                .Should().BeTrue();
        }

        [Test]
        public void LogWriterCallsWriteOnDiskWriteStrategy()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var logRegister = new LogRegister();
            var diskWriteStrategy = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var fileOffsets = Substitute.For<FileOffsets>();

            raftConfiguration.LogPath.Returns("this");
            fileOffsets.GetNextOffset().Returns(0);

            logRegister.AddEncodedLog(@event.Id, BitConverter.GetBytes(1));

            var handler = new LogWriter(raftConfiguration, logRegister, fileOffsets, diskWriteStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            diskWriteStrategy.Received().Write(Arg.Any<string>(), 0, Arg.Any<byte[]>());
        }

        [Test]
        public void LogWriterCallsWriteOnDiskWriteStrategyPassingCorrectPathAndData()
        {
            // Arrange
            const string logPath = "this";
            var data = BitConverter.GetBytes(100);

            var @event = TestEventFactory.GetCommandEvent();
            var logRegister = new LogRegister();
            var diskWriteStrategy = Substitute.For<IWriteToFile>();
            var raftConfiguration = Substitute.For<IRaftConfiguration>();
            var fileOffsets = Substitute.For<FileOffsets>();

            raftConfiguration.LogPath.Returns(logPath);
            fileOffsets.GetNextOffset().Returns(0);

            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(raftConfiguration, logRegister, fileOffsets, diskWriteStrategy);

            // Act
            handler.Handle(@event);

            // Assert
            diskWriteStrategy.Received().Write(Arg.Is(logPath), 0, Arg.Is(data));
        }
    }
}
