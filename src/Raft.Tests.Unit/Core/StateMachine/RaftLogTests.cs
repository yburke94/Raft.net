using System;
using NUnit.Framework;
using Raft.Core.StateMachine.Data;

namespace Raft.Tests.Unit.Core.StateMachine
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
