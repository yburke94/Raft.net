namespace Raft.Core.Commands
{
    internal class SetNewTerm : INodeCommand
    {
        public long Term { get; set; }
    }
}