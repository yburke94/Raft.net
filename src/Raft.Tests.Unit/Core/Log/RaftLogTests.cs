using System;
using NUnit.Framework;
using Raft.Core.Log;

namespace Raft.Tests.Unit.Core.Log
{
    [TestFixture]
    public class RaftLogTests
    {
        [Test]
        public void ThrowsErrorIfCommitIndexIsLessThanOne()
        {
            // Arrange
            var raftLog = new NodeLog();

            // Act, Assert
            Assert.Throws<IndexOutOfRangeException>(() => raftLog.SetLogEntry(0, 10));
        }
    }
}
