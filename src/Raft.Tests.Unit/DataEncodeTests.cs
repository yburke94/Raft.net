using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ProtoBuf;
using Raft.Server;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit
{
    [TestFixture(Ignore = true, IgnoreReason = "Use to generate TestData Encoded Data files.")]
    public class LogEncodeTests
    {
        [Test]
        public void CanEncodeTestCommand()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestCmd-" + DateTime.Now.ToFileTimeUtc() + ".bin");

            using (var file = File.Create(filePath))
            {
                // Act
                Serializer.Serialize(file, new TestCommand());
            }
        }

        [Test]
        public void CanEncodeLogMessage()
        {
            // Arrange
            var log = new LogEntry {
                Index = 1,
                Term = 1,
                CommandType = typeof (TestCommand).AssemblyQualifiedName,
                Command = new TestCommand {
                    Count = 54
                }
            };

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "ProtoEncode-" + DateTime.Now.ToFileTimeUtc() + ".bin");

            using (var file = File.Create(filePath))
            {
                // Act
                Serializer.Serialize(file, log);
            }

            // Assert
            File.ReadAllBytes(filePath).Length.Should().BeGreaterThan(0, "because it should contain encoded data.");
        }

        [Test]
        public void CanDecodeLogMessage()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData\\EncodedData\\EncodedLogEntry.bin");

            using (var file = File.OpenRead(filePath))
            {
                // Act
                var logEntry = Serializer.Deserialize<LogEntry>(file);

                // Assert
                logEntry.Should().NotBeNull();
                logEntry.Index.ShouldBeEquivalentTo(1);
                logEntry.Term.ShouldBeEquivalentTo(1);
                logEntry.Command.Should().BeOfType<TestCommand>();
            }
        }
    }
}
