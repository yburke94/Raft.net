using System;
using Microsoft.Practices.ServiceLocation;
using Raft.Core;

namespace Raft.Server.InternalCommands
{
    public class CreateCluster : IRaftInternalCommand
    {
        public string CommandName { get; private set; }
        public void Execute(IServiceLocator serviceLocator) { }

        public Action<IRaftNode> NodeAction {
            get { return x => x.CreateCluster(); }
        }
    }
}