using System;
using Raft.Server.Services;

namespace Raft.Server
{
    public class PeerNode
    {
        public Guid NodeId { get; set; }

        public IRaftService Channel { get; set; }
    }
}
