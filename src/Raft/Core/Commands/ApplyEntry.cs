namespace Raft.Core.Commands
{
    internal class ApplyEntry : INodeCommand
    {
        public long EntryIdx { get; set; }
    }
}