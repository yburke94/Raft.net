using System;

namespace Raft.Core.Commands
{
    internal class SetLeaderInformation : INodeCommand
    {
        public Guid LeaderId { get; set; }
    }
}
