using System;
using Automatonymous;
using Raft.Core;

namespace Raft.Server
{
    public class JoinCluster : IRaftInternalCommand
    {
        public string CommandName { get; private set; }
        public void Execute(RaftServerContext context)
        {
            // TODO
        }

        public Func<INodeEvents, Event> GetStateMachineEvent {
            get { return x => x.JoinCluster; }
        }
    }
}