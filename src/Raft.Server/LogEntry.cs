using System.Linq;
using ProtoBuf;

namespace Raft.Server
{
    [ProtoContract]
    public class LogEntry
    {
        [ProtoMember(1, IsRequired = true)]
        public long Index { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long Term { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string CommandType { get; set; }

        [ProtoMember(4)]
        public object Command { get; set; }
    }
}
