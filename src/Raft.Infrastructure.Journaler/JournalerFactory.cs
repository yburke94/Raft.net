using System.Collections.Generic;
using Raft.Infrastructure.Journaler.Transformers;
using Raft.Infrastructure.Journaler.Writers;

namespace Raft.Infrastructure.Journaler
{
    public class JournalerFactory
    {
        public IJournaler CreateJournaler(JournalConfiguration configuration)
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

            return new Journaler(configuration, fileWriter, offsetManager, transformers);
        }
    }
}
