using System;
using System.Collections.Generic;
using Raft.Persistance.Journaler.Extensions;

namespace Raft.Persistance.Journaler.Transformers
{
    internal class EntryData : ITransformJournalEntry
    {
        /// <summary>
        /// Journal Layout for a single Entry:
        /// |MetadataLength(int)(0 if null)     |
        /// |Metadata(string)(optional)         |
        /// |Data Length(int)                   |
        /// |Data                               |
        /// |Padding Length(int)(0 if null)     |
        /// |Padding(Unbuffered only)(optional) |
        /// </summary>
        /// <remarks>
        /// Metadata is done by the <see cref="EntryMetadata"/> object.
        /// Padding is done by the <see cref="EntryPadding"/> object.
        /// </remarks>
        public byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata)
        {
            return entryBytes.PrependBytes(BitConverter.GetBytes(entryBytes.Length));
        }
    }
}
