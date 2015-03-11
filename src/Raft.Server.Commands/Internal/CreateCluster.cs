using System;
using Microsoft.Practices.ServiceLocation;
using Raft.Core.StateMachine;

namespace Raft.Server.Commands.Internal
{
    public class CreateCluster : IRaftInternalCommand
    {
        public void Execute(IServiceLocator serviceLocator) { }

        public Action<IRaftNode> NodeAction {
            get { return x => x.CreateCluster(); }
        }
    }
}