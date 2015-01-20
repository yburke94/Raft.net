using System;
using Raft.Infrastructure.Journaler.Extensions;

namespace Raft.Infrastructure.Journaler.Transformers
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
