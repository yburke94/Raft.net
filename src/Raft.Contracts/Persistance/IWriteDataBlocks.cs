namespace Raft.Contracts.Persistance
{
    public interface IWriteDataBlocks
    {
        void WriteBlock(DataBlock block);

        void WriteBlocks(DataBlock[] blocks);
    }
}
