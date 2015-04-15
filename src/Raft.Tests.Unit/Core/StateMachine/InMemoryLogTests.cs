using System;
using FluentAssertions;
using NUnit.Framework;
using Raft.Core.StateMachine.Data;

namespace Raft.Tests.Unit.Core.StateMachine
{
    [TestFixture]
    public class InMemoryLogTests
    {
        [Test]
        public void ThrowsErrorIfCommitIndexIsLessThanOne()
        {
            // Arrange
            var raftLog = new InMemoryLog();

            // Act, Assert
            Assert.Throws<IndexOutOfRangeException>(() => raftLog.SetLogEntry(0, 10));
        }

        [Test]
        public void CanTruncateLogFromGivenIndex()
        {
            // Arrange
            const int initialCount = 10;
            const int truncateFromIndex = 5;

            var nodeLog = new InMemoryLog();
            for (var i = 0; i < initialCount; i++)
                nodeLog.SetLogEntry(i+1, 1);

            // Act
            nodeLog.TruncateLog(truncateFromIndex);

            // Assert
            nodeLog[truncateFromIndex].Should().Be(1);
            for (var i = truncateFromIndex; i < initialCount; i++)
                nodeLog[i + 1].Should().NotHaveValue();
        }
    }
}
