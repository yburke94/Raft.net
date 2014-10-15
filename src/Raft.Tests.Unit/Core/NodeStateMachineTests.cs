using Automatonymous;
using NUnit.Framework;
using Raft.Core;

namespace Raft.Tests.Unit.Core
{
    [TestFixture]
    public class NodeStateMachineTests
    {
        [Test]
        public void CanTransitionToLeaderWhenJoinClusterIsCalled()
        {
            // Arrange
            var node = new Node();
            var nodeStateMachine = new NodeStateMachine();

            // Act
            nodeStateMachine.RaiseEvent(node, nodeStateMachine.JoinCluster);

            // Assert
            Assert.AreEqual(node.CurrentState, nodeStateMachine.Leader);
        }
    }
}
