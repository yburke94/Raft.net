﻿using ProtoBuf;
using Raft.Contracts;

namespace Raft.Server.Data
{
    [ProtoContract]
    internal class LogEntry
    {
        [ProtoMember(1, IsRequired = true)]
        public long Index { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long Term { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string CommandType { get; set; }

        [ProtoMember(4, DynamicType = true)]
        public IRaftCommand Command { get; set; }
    }
}
