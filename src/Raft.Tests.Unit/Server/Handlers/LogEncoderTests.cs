using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Server.Handlers;
using Raft.Server.Log;
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
        public void LogEncoderSkipsInternalCommands()
        {
            // Act, Assert
            typeof (ISkipInternalCommands).IsAssignableFrom(typeof (LogEncoder))
                .Should().BeTrue();
        }

        [Test]
        public void LogEncoderDoesAddEncodedLogToLogRegister()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentTerm.Returns(1);
            raftNode.CommitIndex.Returns(0);

            var logRegister = new LogRegister();

            var handler = new LogEncoder(raftNode, logRegister);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            logRegister.HasLogEntry(@event.Id).Should()
                .BeTrue("because this is where the handler should have placed the encoded log.");
        }

        [Test]
        public void TheEncodedLogDoesMatchTheEncodedLogInTestData()
        {
            // Arrange
            var @event = TestEventFactory.GetCommandEvent();

            var raftNode = Substitute.For<IRaftNode>();
            raftNode.CurrentTerm.Returns(1);
            raftNode.CommitIndex.Returns(0);
            var logRegister = new LogRegister();
            var handler = new LogEncoder(raftNode, logRegister);

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            logRegister.GetEncodedLog(@event.Id)
                .SequenceEqual(_testCommandLogEntryEncoded)
                .Should().BeTrue();
        }
    }
}
