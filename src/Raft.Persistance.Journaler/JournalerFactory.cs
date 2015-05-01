using System.Collections.Generic;
using Raft.Contracts.Persistance;
using Raft.Persistance.Journaler.Readers;
using Raft.Persistance.Journaler.Transformers;
using Raft.Persistance.Journaler.Writers;

namespace Raft.Persistance.Journaler
{
    public class JournalerFactory
    {
        public IWriteDataBlocks CreateJournalWriter(JournalConfiguration configuration)
        {
            var fileWriter = configuration.IoType == IoType.Buffered
                ? (IJournalFileWriter)new BufferedJournalFileWriter(configuration)
                : new UnbufferedJournalFileWriter(configuration);

            var offsetManager = new JournalOffsetManager(configuration);

            var transformers = new List<ITransformJournalEntry>
            {
                new AddJournalMetadata()
            };

            if (configuration.IoType == IoType.Unbuffered)
                transformers.Add(new PadToAlignToSector(configuration));

            return new Journal(configuration, fileWriter, offsetManager, transformers);
        }

        public IReadDataBlocks CreateJournalReader(JournalConfiguration configuration)
        {
            return new JournalReader(configuration);
        }
    }
}
