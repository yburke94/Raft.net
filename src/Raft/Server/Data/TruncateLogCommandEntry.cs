using ProtoBuf;

namespace Raft.Server.Data
{
    [ProtoContract]
    public class TruncateLogCommandEntry
    {
        [ProtoMember(1, IsRequired = true)]
        public long TruncateFromIndex { get; set; }
    }
}
