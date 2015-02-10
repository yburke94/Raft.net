using System;

namespace Raft.Server.Events
{
    public class CommandExecuted
    {
        internal CommandExecuted(bool successful) : this(successful, null) { }

        internal CommandExecuted(bool successful, Exception exception)
        {
            Successful = successful;
            Exception = exception;
        }

        public bool Successful { get; private set; }

        public Exception Exception { get; set; }
    }
}