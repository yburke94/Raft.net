using System.Collections.Generic;
using Raft.Contracts.Persistance;
using Raft.Extensions.Journaler.Transformers;
using Raft.Extensions.Journaler.Writers;

namespace Raft.Extensions.Journaler
{
    public class JournalFactory
    {
        public IWriteDataBlocks CreateJournaler(JournalConfiguration configuration)
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
    }
}
