namespace Raft.Infrastructure.Compression
{
    internal interface IDecompressBlock
    {
        /// <summary>
        /// Determines if the given block has been compressed
        /// via the implementing compression algorithm.
        /// </summary>
        bool HasBeenCompressed(byte[] block);

        /// <summary>
        /// Decompressed the given block via the implementing compression algorithm.
        /// This will throw if the block is not valid for decompression by this algorithm.
        /// </summary>
        byte[] Decompress(byte[] block);
    }
}