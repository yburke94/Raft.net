using System;
using System.Threading;

namespace Raft.Infrastructure
{
    public class WaitableCounter
    {
        private readonly ManualResetEvent _kernelWait;
        private readonly int _limit;

        private int _current = 0;

        public WaitableCounter(int limit)
        {
            _limit = limit;
            _kernelWait = new ManualResetEvent(false);
        }

        public void Increment()
        {
            Interlocked.Increment(ref _current);

            var currCount = Thread.VolatileRead(ref _current);
            if (currCount >= _limit)
                _kernelWait.Set();
        }

        public void Wait()
        {
            var spinWait = new SpinWait();

            while (_current < _limit)
            {
                if (!spinWait.NextSpinWillYield)
                    spinWait.SpinOnce();
                else if (!_kernelWait.WaitOne(Timeout.Infinite))
                    throw new Exception("Failed to wait for Reset event.");
            }
        }
    }
}
