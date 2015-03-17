using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;

namespace Raft.Core.StateMachine
{
    public interface INode
    {
        NodeState CurrentState { get; }
        NodeData Data { get; }
    }
}