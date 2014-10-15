using Automatonymous;
using Raft.Core.Strategies;

namespace Raft.Core
{
    public class Node
    {
        public State CurrentState { get; set; }

        public IApplyCommandStrategy ApplyCommandStrategy { get; set; }

        // State transitions will get and set different strategies on the Node object!
    }
}
