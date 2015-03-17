namespace Raft.Infrastructure.Journaler
{
    public interface IWriteDataBlocks
    {
        void WriteBlock(byte[] block);

        void WriteBlocks(byte[][] blocks);
    }
}
