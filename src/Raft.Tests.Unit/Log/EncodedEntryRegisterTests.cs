using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Raft.Server.Log;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Log
{
    [TestFixture]
    public class EncodedEntryRegisterTests
    {
        [Test]
        public void ThrowsIfTaskThatRepresentsCompletionOfEntryIsNull()
        {
            // Act, Assert
            new Action(() => new EncodedEntryRegister()
                .AddLogEntry(Guid.NewGuid(), 1L, new byte[8], null))
                .ShouldThrow<ArgumentException>("because we need a Task to auto evict entries.");
        }

        [Test]
        public void ThrowsIfTaskThatRepresentsCompletionHasAlreadyCompletedWhenAddingEntry()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);

            // Act, Assert
            new Action(() => new EncodedEntryRegister()
                .AddLogEntry(Guid.NewGuid(), 1L, new byte[8], tcs.Task))
                .ShouldThrow<ArgumentException>("because you should not add the entry if the Task has completed.");
        }

        [TestCase("Fauled")]
        [TestCase("Cancelled")]
        [TestCase("Completed")]
        public void EvictsEncodedEntryUponTaskCompletion(string completionReason)
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var logId = Guid.NewGuid();
            var encodedLogRegister = new EncodedEntryRegister();
            encodedLogRegister.AddLogEntry(logId, 1L, new byte[8], tcs.Task);
            encodedLogRegister.HasLogEntry(logId).Should().BeTrue();

            // Act
            switch (completionReason.ToLower())
            {
                case "fauled":
                    tcs.SetException(new Exception());
                    break;

                case "cancelled":
                    tcs.SetCanceled();
                    break;

                default:
                    tcs.SetResult(0);
                    break;
            }

            // Assert
            encodedLogRegister.HasLogEntry(logId).Should().BeFalse();
        }

        [Test]
        public void CanAddEncodedLog()
        {
            // Arrange
            var id = Guid.NewGuid();
            const long logIdx = 1L;
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new EncodedEntryRegister();

            // Act
            logRegister.AddLogEntry(id, logIdx, encodedLog, TestTask.Create());

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

            var logRegister = new EncodedEntryRegister();
            logRegister.AddLogEntry(id, logIdx, encodedLog, TestTask.Create());

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
            new Action(() => new EncodedEntryRegister()
                .GetEncodedLog(Guid.NewGuid()))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void ThrownWhenAttemptingToAddAnEntryWithTheSameId()
        {
            // Act, Assert
            new Action(() =>
            {
                var id = Guid.NewGuid();
                var register = new EncodedEntryRegister();
                register.AddLogEntry(id, 1L, new byte[3], TestTask.Create());
                register.AddLogEntry(id, 1L, new byte[3], TestTask.Create());
            }).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void CanExplicityEvictLogEntry()
        {
            // Arrange
            var id = Guid.NewGuid();
            var encodedLog = BitConverter.GetBytes(1);
            var logRegister = new EncodedEntryRegister();
            logRegister.AddLogEntry(id, 1L, encodedLog, TestTask.Create());

            // Act
            logRegister.EvictEntry(id);

            // Assert
            logRegister.HasLogEntry(id).Should().BeFalse();
        }
    }
}
