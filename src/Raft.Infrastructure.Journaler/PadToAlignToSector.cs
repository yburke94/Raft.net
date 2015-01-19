
namespace Raft.Infrastructure.Journaler
{
    internal class PadToAlignToSector : ITransformJournalEntry
    {
        private readonly JournalConfiguration _journalConfiguration;

        public PadToAlignToSector(JournalConfiguration journalConfiguration)
        {
            _journalConfiguration = journalConfiguration;
        }

        public byte[] Transform(byte[] block)
        {
            var sectorSize = SectorSize.Get(_journalConfiguration.JournalDirectory);
            var amountToPad = sectorSize - (block.Length % sectorSize);

            return amountToPad == sectorSize
                ? block
                : block.AppendBytes(new byte[amountToPad]);
        }
    }
}