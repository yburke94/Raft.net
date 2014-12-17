namespace Raft.Server
{
    internal interface IMetadataFlushStrategy
    {
        void FlushLogMetadata();
    }
}