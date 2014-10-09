using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raft.Infrastructure
{
    public class TwoPhaseWaitFuture<TResult> : IFuture<TResult> where TResult : class
    {
        private readonly object _resultLock = new object();
        private readonly object _taskLock = new object();

        private readonly ManualResetEvent _kernelEvent = new ManualResetEvent(false);

        // 0 = NoResult, 1 = Result
        private int _futureState = 0;

        private volatile TResult _result;
        private volatile Task<TResult> _callBackTask;

        public bool HasResult()
        {
            return _futureState > 0;
        }

        /// <summary>
        /// Will block until the result is available.
        /// </summary>
        /// <remarks>Will spin until to OS forces a yeild. Then the kernel will be responsible for signaling the completeion of the operation.</remarks>
        public TResult Result()
        {
            return Result(Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Will block until the result is available or until the timeout has expired.
        /// </summary>
        /// <remarks>Will spin until to OS forces a yeild. Then the kernel will be responsible for signaling the completeion of the operation.</remarks>
        public TResult Result(TimeSpan timeout)
        {
            var isInfinite = timeout.Equals(Timeout.InfiniteTimeSpan);
            var started = DateTime.Now;
            var spinWait = new SpinWait();

            while (!HasResult())
            {
                if (!spinWait.NextSpinWillYield)
                {
                    spinWait.SpinOnce();

                    if (!isInfinite && (DateTime.Now >= started.Add(timeout)))
                        throw new TimeoutException("Timed out waiting for result.");

                    continue;
                }

                var remaining = isInfinite
                    ? -1
                    : (int)(timeout - (DateTime.Now - started)).TotalMilliseconds;

                if ((remaining <=  0 && !isInfinite) || !_kernelEvent.WaitOne(remaining))
                    throw new TimeoutException("Timed out waiting for result.");
            }

            return _result;
        }

        /// <summary>
        /// Wlll register a task to execute once the result is available
        /// </summary>
        public Task Register(Action<Task<TResult>> callback)
        {
            if (_callBackTask == null)
            {
                lock (_taskLock)
                {
                    if (_callBackTask == null)
                    {
                        _callBackTask = Task.Factory.StartNew(() => Result());
                    }
                }
            }

            return _callBackTask.ContinueWith(callback);
        }

        public void SetResult(TResult result)
        {
            if (_result != null)
                throw new InvalidOperationException("Future result has already been set.");

            lock (_resultLock)
            {
                if (_result != null)
                    throw new InvalidOperationException("Future result has already been set.");

                _result = result;
                _futureState = 1;
                _kernelEvent.Set();
            }
        }
    }
}
