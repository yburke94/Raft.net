namespace Raft.Core.Commands
{
    public class SetNewTerm : INodeCommand
    {
        public long Term { get; set; }
    }
}