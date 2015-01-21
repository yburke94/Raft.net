using System;
using NUnit.Framework;
using Raft.Core;

namespace Raft.Tests.Unit.Core
{
    [TestFixture]
    public class RaftLogTests
    {
        [Test]
        public void ThrowsErrorIfCommitIndexIsLessThanOne()
        {
            // Arrange
            var raftLog = new RaftLog();

            // Act, Assert
            Assert.Throws<IndexOutOfRangeException>(() => raftLog.SetLogEntry(0, 10));
        }
    }
}
