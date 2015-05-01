using System.Collections.Generic;
using Raft.Persistance.Journaler.Extensions;
using Raft.Persistance.Journaler.Kernel;

namespace Raft.Persistance.Journaler.Transformers
{
    /// <summary>
    /// Should only be used when writing to an unbuffered FileStream.
    /// </summary>
    internal class PadToAlignToSector : ITransformJournalEntry
    {
        private readonly JournalConfiguration _journalConfiguration;

        public PadToAlignToSector(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

        /// <summary>
        /// Journal Layout for a single Entry:
        /// |Data Length(int)               |
        /// |MetadataLength(int)(0 if null) |
        /// |Metadata(string)(optional)     |
        /// |Data                           |
        /// |Padding(optional)              |
        /// </summary>
        /// <remarks>
        /// Metadata is done by the <see cref="AddJournalMetadata"/> object.
        /// </remarks>
        public byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata)
        {
            var sectorSize = SectorSize.Get(_journalConfiguration.JournalDirectory);
            var amountToPad = sectorSize - (entryBytes.Length % sectorSize);

            return amountToPad == sectorSize
                ? entryBytes
                : entryBytes.AppendBytes(new byte[amountToPad]);
        }
    }
}