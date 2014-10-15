namespace Raft.Core.Strategies
{
    public interface IApplyCommandStrategy
    {
        void ApplyCommandToLog();
    }
}