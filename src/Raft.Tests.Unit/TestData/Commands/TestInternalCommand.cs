using System;
using Microsoft.Practices.ServiceLocation;
using ProtoBuf;
using Raft.Core;
using Raft.Server.InternalCommands;

namespace Raft.Tests.Unit.TestData.Commands
{
    [ProtoContract]
    public class TestInternalCommand : IRaftInternalCommand
    {
        [ProtoMember(1)]
        public int Count { get; set; }

        public void Execute(IServiceLocator serviceLocator)
        {
            // Do Nothing!
        }

        public Action<IRaftNode> NodeAction {
            get { return x => x.CreateCluster(); }
        }
    }
}
