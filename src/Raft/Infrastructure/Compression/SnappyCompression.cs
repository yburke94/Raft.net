using Snappy;

namespace Raft.Infrastructure.Compression
{
    /// <summary>
    /// Uses Snappy.NET for Compression and Decompression.
    /// The Snappy.NET library internally uses P/Invokes to call into the Snappy C libraries.
    /// </summary>
    internal class SnappyCompression : ICompressBlock, IDecompressBlock
    {
        public byte[] Compress(byte[] block)
        {
            return SnappyCodec.Compress(block);
        }

        public bool HasBeenCompressed(byte[] block)
        {
            return SnappyCodec.Validate(block);
        }

        public byte[] Decompress(byte[] block)
        {
            return SnappyCodec.Uncompress(block);
        }
    }
}
