using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogEncoderTests
    {
        private byte[] _testCommandLogEntryEncoded;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _testCommandLogEntryEncoded =
                File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "TestData\\EncodedData\\EncodedLogEntry.bin"));
        }

        [Test]
        public void DoesNotAddEncodedLogToEventMetadataWhenHandlingInternalCommand()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .ResetEvent(new TestInternalCommand(), new TaskCompletionSource<LogResult>());

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentLogTerm.Returns(1);
            raftNode.LastLogIndex.Returns(0);

            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata.ContainsKey("EncodedLog")
                .Should().BeFalse("because the handler should not encode internal commands.");
        }

        [Test]
        public void DoesAddEncodedLogToEventMetadata()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .ResetEvent(new TestCommand(), new TaskCompletionSource<LogResult>());

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentLogTerm.Returns(1);
            raftNode.LastLogIndex.Returns(0);


            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata.ContainsKey("EncodedLog")
                .Should().BeTrue("because this is where the handler should have placed the encoded log.");

            @event.Metadata["EncodedLog"].Should().NotBeNull();
            @event.Metadata["EncodedLog"].Should().BeOfType<byte[]>();
        }

        [Test]
        public void TheEncodedLogDoesMatchTheEncodedLogInTestData()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .ResetEvent(new TestCommand(), new TaskCompletionSource<LogResult>());

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentLogTerm.Returns(1);
            raftNode.LastLogIndex.Returns(0);

            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata["EncodedLog"]
                .As<byte[]>()
                .SequenceEqual(_testCommandLogEntryEncoded)
                .Should().BeTrue();
        }
    }
}
