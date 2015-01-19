using System;

namespace Raft.Infrastructure.Journaler
{
    internal class AddJournalMetadata : ITransformJournalEntry
    {
        public byte[] Transform(byte[] block)
        {
            var lengthBytes = BitConverter.GetBytes(block.Length);
            return block.PrependBytes(lengthBytes);
        }
    }
}
