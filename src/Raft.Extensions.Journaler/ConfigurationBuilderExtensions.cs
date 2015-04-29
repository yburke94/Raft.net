using System;
using Raft.Configuration;
using Raft.Contracts.Persistance;

namespace Raft.Extensions.Journaler
{
    public static class ConfigurationBuilderExtensions
    {
        public static RaftConfigurationBuilder WithJournalPersistance(
            this RaftConfigurationBuilder configBuilder,
            JournalConfiguration config)
        {
            Func<IWriteDataBlocks> writerFactory =
                () => new JournalerFactory().CreateJournalWriter(config);

            Func<IReadDataBlocks> readerFactory =
                () => null; // TODO: Implement

            return configBuilder.WithPersistance(writerFactory, readerFactory);
        }
    }
}
