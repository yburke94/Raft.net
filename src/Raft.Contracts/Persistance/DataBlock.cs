using System.Collections.Generic;

namespace Raft.Contracts.Persistance
{
    public class DataBlock
    {
        public IDictionary<string, string> Metadata { get; set; }

        public byte[] Data { get; set; }
    }
}