namespace Raft.Core.Events
{
    internal class TermChanged
    {
        public TermChanged(long newTerm)
        {
            NewTerm = newTerm;
        }

        public long NewTerm { get; private set; }
    }
}
