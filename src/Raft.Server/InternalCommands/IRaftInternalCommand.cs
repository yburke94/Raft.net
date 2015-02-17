using System;
using Raft.Core;

namespace Raft.Server.InternalCommands
{
    internal interface IRaftInternalCommand : IRaftCommand
    {
        Action<IRaftNode> NodeAction { get; }
    }
}