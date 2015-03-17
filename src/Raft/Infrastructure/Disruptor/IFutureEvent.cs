using System.Threading.Tasks;

namespace Raft.Infrastructure.Disruptor
{
    internal interface IFutureEvent<T>
    {
        TaskCompletionSource<T> CompletionSource { get; }
    }
}