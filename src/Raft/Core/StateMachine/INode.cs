using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;

namespace Raft.Core.StateMachine
{
    internal interface INode
    {
        /// <summary>
        /// The current state of the node.
        /// </summary>
        NodeState CurrentState { get; }

        /// <summary>
        /// Contains both persisted and volatile properties of the node.
        /// </summary>
        NodeProperties Properties { get; }

        /// <summary>
        /// In memory representation of the committed log.
        /// </summary>
        InMemoryLog Log { get; }
    }
}