using System.Collections.Generic;

namespace Raft.Infrastructure.Journaler.Transformers
{
    internal interface ITransformJournalEntry
    {
        byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata);
    }
}
