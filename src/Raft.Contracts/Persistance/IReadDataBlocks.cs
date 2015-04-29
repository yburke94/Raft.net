namespace Raft.Contracts.Persistance
{
    // TODO: IMPL
    public interface IReadDataBlocks
    {
        DataBlock GetBlock(DataRequest request);
    }

    public class DataRequest
    {
        public DataRequest(long index)
        {
            
        }
    }
}
