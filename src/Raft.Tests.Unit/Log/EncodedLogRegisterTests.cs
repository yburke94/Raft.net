using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Raft.Server.Log;

namespace Raft.Tests.Unit.Log
{
    [TestFixture]
    public class EncodedLogRegisterTests
    {
        [Test]
        public void CanAddEncodedLog()
        {
            // Arrange
            var id = Guid.NewGuid();
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new EncodedLogRegister(1);

            // Act
            logRegister.AddEncodedLog(id, encodedLog);

            // Assert
            logRegister.HasLogEntry(id).Should().BeTrue();
        }

        [Test]
        public void ThrownWhenAttemptingToAccessAnEntryWhichHasNotBeenSaved()
        {
            // Act, Assert
            new Action(() => new EncodedLogRegister(1).GetEncodedLog(Guid.NewGuid()))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void ThrownWhenAttemptingToAddAnEntryWithTheSameId()
        {
            // Act, Assert
            new Action(() =>
            {
                var id = Guid.NewGuid();
                var register = new EncodedLogRegister(1);
                register.AddEncodedLog(id, new byte[3]);
                register.AddEncodedLog(id, new byte[3]);
            }).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void CanExplicityEvictLogEntry()
        {
            // Arrange
            var id = Guid.NewGuid();
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(id, encodedLog);

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
            var logRegister = new EncodedLogRegister(maxAccessTimes);
            logRegister.AddEncodedLog(id, encodedLog);

            // Act
            logRegister.GetEncodedLog(id);
            logRegister.GetEncodedLog(id);

            // Assert
            logRegister.HasLogEntry(id).Should().BeFalse();
        }
    }
}
