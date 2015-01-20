using System;
using Raft.Core;

namespace Raft.Server.Commands
{
    internal interface IRaftInternalCommand : IRaftCommand
    {
        Action<IRaftNode> NodeAction { get; }
    }
}