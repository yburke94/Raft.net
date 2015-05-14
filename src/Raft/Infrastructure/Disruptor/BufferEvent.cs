using System.Threading.Tasks;

namespace Raft.Infrastructure.Disruptor
{
    internal abstract class BufferEvent
    {
        protected BufferEvent()
        {
            CompletionSource = new TaskCompletionSource<object>();
        }

        public TaskCompletionSource<object> CompletionSource { get; protected set; }

        public bool HasCompletedSuccessfully()
        {
            return CompletionSource.Task.IsCompleted && !HasFaulted();
        }

        public bool HasFaulted()
        {
            return CompletionSource.Task.IsFaulted;
        }

        public void CompleteEvent()
        {
            if(CompletionSource != null)
                CompletionSource.SetResult(new {Success = true});
        }
    }
}