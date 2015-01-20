using System;
using Raft.Core;

namespace Raft.Server.Commands.Internal
{
    public class JoinCluster : IRaftInternalCommand
    {
        public string CommandName { get; private set; }
        public void Execute(RaftServerContext context) { }

        public Action<IRaftNode> NodeAction {
            get { return x => x.JoinCluster(); }
        }
    }
}