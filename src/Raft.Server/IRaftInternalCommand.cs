using System;
using Raft.Core;

namespace Raft.Server
{
    internal interface IRaftInternalCommand : IRaftCommand
    {
        Action<IRaftNode> NodeAction { get; }
    }
}