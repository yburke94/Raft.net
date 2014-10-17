using System;
using Automatonymous;
using Raft.Core;

namespace Raft.Server
{
    internal interface IRaftInternalCommand : IRaftCommand
    {
        Func<INodeEvents, Event> GetStateMachineEvent { get; }
    }
}