namespace Raft.Core.Commands
{
    public class ApplyEntry : INodeCommand
    {
        public long EntryIdx { get; set; }
    }
}