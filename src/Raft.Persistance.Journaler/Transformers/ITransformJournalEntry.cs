using System.Collections.Generic;

namespace Raft.Persistance.Journaler.Transformers
{
    internal interface ITransformJournalEntry
    {
        byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata);
    }
}
