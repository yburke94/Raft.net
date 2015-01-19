namespace Raft.Infrastructure.Journaler
{
    public interface IJournaler
    {
        void WriteBlock(byte[] block);

        void WriteBlocks(byte[][] blocks);
    }
}
