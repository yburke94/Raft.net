namespace Raft.Core.Commands
{
    internal class TruncateLog : INodeCommand
    {
        public long TruncateFromIndex { get; set; }
    }
}
