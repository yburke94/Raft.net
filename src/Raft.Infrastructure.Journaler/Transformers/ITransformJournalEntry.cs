using System.Collections.Generic;

namespace Raft.Extensions.Journaler.Transformers
{
    internal interface ITransformJournalEntry
    {
        byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata);
    }
}
