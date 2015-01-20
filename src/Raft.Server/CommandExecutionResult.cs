using System;

namespace Raft.Server
{
    public class CommandExecutionResult
    {
        internal CommandExecutionResult(bool successful) : this(successful, null) { }

        internal CommandExecutionResult(bool successful, Exception exception)
        {
            Successful = successful;
            Exception = exception;
        }

        public bool Successful { get; private set; }

        public Exception Exception { get; set; }
    }
}