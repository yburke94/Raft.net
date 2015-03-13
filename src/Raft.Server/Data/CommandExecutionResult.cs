using System;

namespace Raft.Server.Data
{
    public class CommandExecutionResult
    {
        public CommandExecutionResult(bool successful) : this(successful, null) { }

        public CommandExecutionResult(bool successful, Exception exception)
        {
            Successful = successful;
            Exception = exception;
        }

        public bool Successful { get; private set; }

        public Exception Exception { get; set; }
    }
}