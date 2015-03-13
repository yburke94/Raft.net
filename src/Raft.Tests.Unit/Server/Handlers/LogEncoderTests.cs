using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.StateMachine;
using Raft.Server.Data;
using Raft.Server.Handlers.Leader;
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
        public void LogEncoderDoesAddEncodedLogToLogEvent()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentTerm.Returns(1);
            raftNode.CommitIndex.Returns(0);

            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.EncodedEntry.Should().NotBeNull();
        }

        [Test]
        public void LogEncoderDoesAddLogEntryToLogRegisterWithIndexTermAndCommandTypeSet()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();
            var expectedLogEntry = new LogEntry
            {
                Term = 3L,
                Index = 7L,
                CommandType = @event.Command.GetType().AssemblyQualifiedName,
                Command = @event.Command
            };

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentTerm.Returns(expectedLogEntry.Term);
            raftNode.CommitIndex.Returns(expectedLogEntry.Index-1);

            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.LogEntry.Should().NotBeNull();
            @event.LogEntry.Term.Should().Be(expectedLogEntry.Term);
            @event.LogEntry.Index.Should().Be(expectedLogEntry.Index);
            @event.LogEntry.CommandType.Should().Be(expectedLogEntry.CommandType);
            @event.LogEntry.Command.Should().Be(expectedLogEntry.Command);
        }

        [Test]
        public void TheEncodedLogDoesMatchTheEncodedLogInTestData()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentTerm.Returns(1);
            raftNode.CommitIndex.Returns(0);
            var handler = new LogEncoder(raftNode);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.EncodedEntry
                .SequenceEqual(_testCommandLogEntryEncoded)
                .Should().BeTrue();
        }
    }
}
