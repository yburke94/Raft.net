namespace Raft.Core.Commands
{
    public class CommitEntry : INodeCommand
    {
        public long EntryIdx { get; set; }
        public long EntryTerm { get; set; }
    }
}