using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Core.Data;

namespace Raft.Tests.Unit.Core.Data
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
            Assert.Throws<IndexOutOfRangeException>(() => raftLog.SetLogEntry(0, 10, new byte[0]));
        }

        [Test]
        public void CanSetEntryAtIndex()
        {
            // Arrange
            const long commitIdx = 1L;
            var raftLog = new InMemoryLog();

            // Act
            raftLog.SetLogEntry(commitIdx, 1L, new byte[0]);

            // Assert
            raftLog.HasEntry(1L).Should().BeTrue();
        }

        [Test]
        public void CanRetreiveTermAtIndex()
        {
            // Arrange
            const long commitIdx = 1L;
            const long commitTerm = 3L;
            var raftLog = new InMemoryLog();
            raftLog.SetLogEntry(commitIdx, commitTerm, new byte[0]);

            // Act
            var term = raftLog.GetTermForEntry(commitIdx);

            // Assert
            term.Should().HaveValue();
            term.Should().Be(commitTerm);
        }

        [Test]
        public void CanRetreiveEntryAtIndex()
        {
            // Arrange
            const long commitIdx = 1L;
            const long commitTerm = 3L;
            var entry = BitConverter.GetBytes(100);

            var raftLog = new InMemoryLog();
            raftLog.SetLogEntry(commitIdx, commitTerm, entry);

            // Act
            var result = raftLog.GetLogEntry(commitIdx);

            // Assert
            result.Should().NotBeNull();
            result.SequenceEqual(entry).Should().BeTrue();
        }

        [Test]
        public void ThrowsIfEntryIsNullWhenSeetingLogEntry()
        {
            // Arrange
            const long commitIdx = 1L;
            const long commitTerm = 3L;

            var raftLog = new InMemoryLog();
            
            // Act
            Action actAction = () => raftLog.SetLogEntry(commitIdx, commitTerm, null);

            // Assert
            actAction.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void CanTruncateLogFromGivenIndex()
        {
            // Arrange
            const int initialCount = 10;
            const int truncateFromIndex = 5;

            var nodeLog = new InMemoryLog();
            for (var i = 0; i < initialCount; i++)
                nodeLog.SetLogEntry(i + 1, 1, new byte[0]);

            // Act
            nodeLog.TruncateLog(truncateFromIndex);

            // Assert
            nodeLog.GetTermForEntry(truncateFromIndex).Should().Be(1);
            for (var i = truncateFromIndex; i < initialCount; i++)
                nodeLog.HasEntry(i+1).Should().BeFalse();
        }
    }
}
