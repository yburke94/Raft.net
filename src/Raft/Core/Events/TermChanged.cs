namespace Raft.Core.Events
{
    public class TermChanged
    {
        public TermChanged(long newTerm)
        {
            NewTerm = newTerm;
        }

        public long NewTerm { get; private set; }
    }
}
