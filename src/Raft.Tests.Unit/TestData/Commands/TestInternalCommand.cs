using System;
using ProtoBuf;
using Raft.Core;
using Raft.Server;
using Raft.Server.Commands;

namespace Raft.Tests.Unit.TestData.Commands
{
    [ProtoContract]
    public class TestInternalCommand : IRaftInternalCommand
    {
        [ProtoMember(1)]
        public int Count { get; set; }

        public void Execute(RaftServerContext context)
        {
            // Do Nothing!
        }

        public Action<IRaftNode> NodeAction {
            get { return x => x.JoinCluster(); }
        }
    }
}
