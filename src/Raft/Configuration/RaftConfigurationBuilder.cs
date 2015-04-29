using System;
using Raft.Contracts.Persistance;

namespace Raft.Configuration
{
    public class RaftConfigurationBuilder
    {
        public RaftConfigurationBuilder WithPersistance(Func<IWriteDataBlocks> blockWriterFactory,
            Func<IReadDataBlocks> blockReaderFactory)
        {
            throw new NotImplementedException();
        }
    }
}