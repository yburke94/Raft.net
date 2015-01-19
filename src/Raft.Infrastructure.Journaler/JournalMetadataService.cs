using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Infrastructure.Journaler
{
    internal class JournalMetadataService : IGetJournalEntryMetadata, ISetJournalEntryMetadata
    {
        public EntryMetadata GetMetadata(FileStream stream)
        {
            throw new NotImplementedException();
        }

        public byte[] SetMetadata(EntryMetadata metadata, byte[] journalEntry)
        {
            var lengthBytes = BitConverter.GetBytes(metadata.Length);
            var paddingBytes = BitConverter.GetBytes(metadata.Padding);

            var journalEntryWithMetadata = new byte[lengthBytes.Length + paddingBytes.Length + journalEntry.Length];
            lengthBytes.CopyTo(journalEntryWithMetadata, 0);
            paddingBytes.CopyTo(journalEntryWithMetadata, lengthBytes.Length-1);
            journalEntry.CopyTo(journalEntryWithMetadata, (lengthBytes.Length-1)+(paddingBytes.Length-1));

            return journalEntryWithMetadata;
        }
    }
}
