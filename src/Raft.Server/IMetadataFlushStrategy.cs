namespace Raft.Server.Handlers
{
    internal interface IMetadataFlushStrategy
    {
        void FlushLogMetadata();
    }
}