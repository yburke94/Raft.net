using System;

namespace Raft.Server
{
    public class LogResult
    {
        internal LogResult(bool successful) : this(successful, null) { }

        internal LogResult(bool successful, Exception exception)
        {
            Successful = successful;
            Exception = exception;
        }

        public bool Successful { get; private set; }

        public Exception Exception { get; set; }
    }
}