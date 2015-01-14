using System.IO;

namespace Raft.Infrastructure.Journaler
{
    internal class SectorSizeEntryPadder : IJournalEntryPadder
    {
        private readonly JournalConfiguration _journalConfiguration;

        public SectorSizeEntryPadder(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

        public byte[] AddPaddingToEntry(byte[] entry)
        {
            var sectorSize = SectorSize.Get(Path.GetPathRoot(_journalConfiguration.JournalDirectory));
            var amountToPad = sectorSize - (entry.Length % sectorSize);

            if (amountToPad == sectorSize)
                return entry;

            var newBlockToWrite = new byte[entry.Length + amountToPad];
            entry.CopyTo(newBlockToWrite, 0);

            for (var i = entry.Length - 1; i < newBlockToWrite.Length; i++)
                newBlockToWrite[i] = byte.MinValue;

            return newBlockToWrite;
        }
    }
}