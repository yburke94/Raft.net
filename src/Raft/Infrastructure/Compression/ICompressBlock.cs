namespace Raft.Infrastructure.Compression
{
    internal interface ICompressBlock
    {
        /// <summary>
        /// Compressed the given block via the implementing compression algorithm.
        /// </summary>
        byte[] Compress(byte[] block);
    }
}