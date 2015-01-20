using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Infrastructure.Journaler;
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
        public void WritesDataBlockToJournaler()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);
            
            var @event = TestEventFactory.GetCommandEvent();

            var journaler = Substitute.For<IJournaler>();

            var logRegister = new LogRegister();
            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(logRegister, journaler);

            // Act
            handler.Handle(@event);

            // Assert+
            journaler.Received()
                .WriteBlock(data);
        }
    }
}
