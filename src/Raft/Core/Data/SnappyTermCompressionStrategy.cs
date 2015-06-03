using Raft.Infrastructure;
using Raft.Infrastructure.Compression;

namespace Raft.Core.Data
{
    internal class SnappyTermCompressionStrategy : ITermCompressionStrategy
    {
        private readonly SnappyCompression _snappyCompression;

        public SnappyTermCompressionStrategy(SnappyCompression snappyCompression)
        {
            _snappyCompression = snappyCompression;
        }

        public byte[] Compress(Ziplist lastTermLog)
        {
            var zipListBytes = lastTermLog.GetBytes();
            return _snappyCompression.Compress(zipListBytes);
        }

        public Ziplist Decompress(byte[] compressedBytes)
        {
            var zipListBytes = _snappyCompression.Decompress(compressedBytes);
            return Ziplist.FromBytes(zipListBytes);
        }
    }
}