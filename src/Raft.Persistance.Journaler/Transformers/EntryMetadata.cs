using System;
using System.Collections.Generic;
using System.Text;
using Raft.Persistance.Journaler.Extensions;

namespace Raft.Persistance.Journaler.Transformers
{
    internal class EntryMetadata : ITransformJournalEntry
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
        /// Padding is done by the <see cref="EntryPadding"/> object.
        /// Data is done by the <see cref="EntryData"/> object.
        /// </remarks>
        public byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata)
        {
            var metadataString = entryMetadata.Stringify();
            var metadataBytes = Encoding.Default.GetBytes(metadataString);

            var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

            return entryBytes
                .PrependBytes(metadataBytes) // Write Metadata
                .PrependBytes(metadataLength); // Write Metadata Length
        }
    }
}
