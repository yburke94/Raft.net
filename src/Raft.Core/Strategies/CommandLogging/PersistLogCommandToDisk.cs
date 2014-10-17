using Raft.Core.Strategies;

namespace Raft.Core.CommandLogging
{
    public class PersistLogCommandToDisk : IApplyCommandStrategy
    {
        public void ApplyCommandToLog() { }
    }
}