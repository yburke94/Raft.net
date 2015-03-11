using System;
using Raft.Core.StateMachine;

namespace Raft.Server.Commands.Internal
{
    public interface IRaftInternalCommand : IRaftCommand
    {
        Action<IRaftNode> NodeAction { get; }
    }
}