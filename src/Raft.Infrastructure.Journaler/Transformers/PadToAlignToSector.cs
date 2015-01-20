
using Raft.Infrastructure.Journaler.Extensions;
using Raft.Infrastructure.Journaler.KernelHelpers;

namespace Raft.Infrastructure.Journaler.Transformers
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