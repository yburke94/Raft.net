namespace Raft.Core.Commands
{
    internal class CommitEntry : INodeCommand
    {
        public long EntryIdx { get; set; }
        public long EntryTerm { get; set; }
    }
}