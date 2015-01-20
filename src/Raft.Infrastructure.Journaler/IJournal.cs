namespace Raft.Infrastructure.Journaler
{
    public interface IJournal
    {
        void WriteBlock(byte[] block);

        void WriteBlocks(byte[][] blocks);
    }
}
