using System;
using System.Collections.Generic;
using System.Text;
using Raft.Infrastructure.Journaler.Extensions;

namespace Raft.Infrastructure.Journaler.Transformers
{
    internal class AddJournalMetadata : ITransformJournalEntry
    {
        /// <summary>
        /// Journal Layout for a single Entry:
        /// |Data Length(int)               |
        /// |MetadataLength(int)(0 if null) |
        /// |Metadata(string)(optional)     |
        /// |Data                           |
        /// |Padding(optional)              |
        /// </summary>
        /// <remarks>
        /// Padding is done by the <see cref="PadToAlignToSector"/> object.
        /// </remarks>
        public byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata)
        {
            var metadataString = entryMetadata.Stringify();
            var metadataBytes = Encoding.Default.GetBytes(metadataString);
            
            var dataLength = BitConverter.GetBytes(entryBytes.Length);
            var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

            return entryBytes
                .PrependBytes(metadataBytes) // Write Metadata
                .PrependBytes(metadataLength) // Write Metadata Length
                .PrependBytes(dataLength);
        }
    }
}
