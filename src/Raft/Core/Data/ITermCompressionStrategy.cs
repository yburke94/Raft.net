using Raft.Infrastructure;

namespace Raft.Core.Data
{
    internal interface ITermCompressionStrategy
    {
        byte[] Compress(Ziplist lastTermLog);

        Ziplist Decompress(byte[] compressedBytes);
    }
}