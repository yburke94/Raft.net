using System;
using System.Collections.Generic;
using Raft.Persistance.Journaler.Extensions;
using Raft.Persistance.Journaler.Kernel;

namespace Raft.Persistance.Journaler.Transformers
{
    /// <summary>
    /// Aligned block to a sector boundary when writing to an unbuffered FileStream.
    /// </summary>
    internal class EntryPadding : ITransformJournalEntry
    {
        private readonly JournalConfiguration _journalConfiguration;

        public EntryPadding(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

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
        /// Data is done by the <see cref="EntryData"/> object.
        /// </remarks>
        public byte[] Transform(byte[] entryBytes, IDictionary<string, string> entryMetadata)
        {
            if (_journalConfiguration.IoType == IoType.Buffered)
                return entryBytes.AppendBytes(BitConverter.GetBytes(0));

            var sectorSize = SectorSize.Get(_journalConfiguration.JournalDirectory);
            var amountToPad = (int)(sectorSize - ((entryBytes.Length + sizeof(int)/*To account for padding length*/) % sectorSize));

            var paddedBytesLength = BitConverter.GetBytes(amountToPad);

            return amountToPad == sectorSize
                ? entryBytes.AppendBytes(BitConverter.GetBytes(0)) // Padding Length.
                : entryBytes
                    .AppendBytes(paddedBytesLength)
                    .AppendBytes(new byte[amountToPad]);
        }
    }
}