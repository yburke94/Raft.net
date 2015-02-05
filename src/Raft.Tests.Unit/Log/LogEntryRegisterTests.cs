using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Server.Log;

namespace Raft.Tests.Unit.Log
{
    [TestFixture]
    public class LogEntryRegisterTests
    {
        [Test]
        public void CanAddEncodedLog()
        {
            // Arrange
            var id = Guid.NewGuid();
            const long logIdx = 1L;
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new LogEntryRegister(1);

            // Act
            logRegister.AddEncodedLog(id, logIdx, encodedLog);

            // Assert
            logRegister.HasLogEntry(id).Should().BeTrue();
        }

        [Test]
        public void CanRetrieveEncodedLog()
        {
            // Arrange
            var id = Guid.NewGuid();
            const long logIdx = 1L;
            var encodedLog = BitConverter.GetBytes(1);

            var logRegister = new LogEntryRegister(1);
            logRegister.AddEncodedLog(id, logIdx, encodedLog);

            // Act
            var entry = logRegister.GetEncodedLog(id);

            // Assert
            entry.Key.ShouldBeEquivalentTo(logIdx);
            entry.Value.SequenceEqual(encodedLog).Should().BeTrue();
        }

        [Test]
        public void ThrownWhenAttemptingToAccessAnEntryWhichHasNotBeenSaved()
        {
            // Act, Assert
            new Action(() => new LogEntryRegister(1).GetEncodedLog(Guid.NewGuid()))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void ThrownWhenAttemptingToAddAnEntryWithTheSameId()
        {
            // Act, Assert
            new Action(() =>
            {
                var id = Guid.NewGuid();
                var register = new LogEntryRegister(1);
                register.AddEncodedLog(id, 1L, new byte[3]);
                register.AddEncodedLog(id, 1L, new byte[3]);
            }).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void CanExplicityEvictLogEntry()
        {
            // Arrange
            var id = Guid.NewGuid();
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new LogEntryRegister(1);
            logRegister.AddEncodedLog(id, 1L, encodedLog);

            // Act
            logRegister.EvictEntry(id);

            // Assert
            logRegister.HasLogEntry(id).Should().BeFalse();
        }

        [Test]
        public void WillAutoEvictLogEntryAfterNTimesAccessed()
        {
            // Arrange
            const int maxAccessTimes = 2;

            var id = Guid.NewGuid();
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new LogEntryRegister(maxAccessTimes);
            logRegister.AddEncodedLog(id, 1L, encodedLog);

            // Act
            logRegister.GetEncodedLog(id);
            logRegister.GetEncodedLog(id);

            // Assert
            logRegister.HasLogEntry(id).Should().BeFalse();
        }
    }
}
