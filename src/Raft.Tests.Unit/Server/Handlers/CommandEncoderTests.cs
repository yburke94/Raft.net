using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Server;
using Raft.Server.Handlers;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class CommandEncoderTests
    {
        private byte[] _testCommandEncoded;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _testCommandEncoded =
                File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "TestData\\EncodedData\\EncodedTestCommand.bin"));
        }

        [Test]
        public void DoesNotAddInternalCommandToEventMetadata()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .Copy(new CommandScheduledEvent
                {
                    Command = new TestInternalCommand(),
                    WhenLogged = _ => { }
                });

            var handler = new CommandEncoder();

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata.ContainsKey("EncodedCommand")
                .Should().BeFalse("because the handler should not encode internal commands.");
        }

        [Test]
        public void DoesAddEncodedCommandToEventMetadata()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .Copy(new CommandScheduledEvent {
                    Command = new TestCommand(),
                    WhenLogged = _ => { }
                });

            var handler = new CommandEncoder();

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata.ContainsKey("EncodedCommand")
                .Should().BeTrue("because this is where the handler should have placed the encoded command.");

            @event.Metadata["EncodedCommand"].Should().NotBeNull();
            @event.Metadata["EncodedCommand"].Should().BeOfType<byte[]>();
        }

        [Test]
        public void TheEncodedTestCommandDoesMatchTheEncodedTestCommandInTestData()
        {
            // Arrange
            var @event = new CommandScheduledEvent()
                .Copy(new CommandScheduledEvent {
                    Command = new TestCommand(),
                    WhenLogged = _ => { }
                });
            var handler = new CommandEncoder();

            // Act
            handler.OnNext(@event, 1, false);

            // Assert
            @event.Metadata["EncodedCommand"]
                .As<byte[]>()
                .SequenceEqual(_testCommandEncoded)
                .Should().BeTrue();
        }
    }
}
