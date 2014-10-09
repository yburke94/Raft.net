using System;
using System.Threading.Tasks;

namespace Raft.Infrastructure
{
    public interface IFuture<TResult> where TResult : class
    {
        bool HasResult();

        /// <summary>
        /// Will block unil the result is available.
        /// </summary>
        /// <remarks>Will spin until to OS forces a yeild. Then the kernel will be responsible for signaling the completeion of the operation.</remarks>
        TResult Result();

        /// <summary>
        /// Wlll register a task to execute once the result is available
        /// </summary>
        Task Register(Action<Task<TResult>> callback);
    }
}